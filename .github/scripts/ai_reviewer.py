import json
import os
import sys
import time
from typing import Any

import requests


def get_env_or_exit(name: str) -> str:
    value = os.getenv(name)
    if value:
        return value
    # PR_NUMBER opsiyonel olabilir push durumunda
    if name == "PR_NUMBER":
        return ""
    print(f"❌ Hata: {name} ortam değişkeni bulunamadı.")
    sys.exit(1)


def read_diff(path: str) -> str:
    if not os.path.exists(path):
        print(f"❌ Hata: {path} bulunamadı.")
        sys.exit(1)

    with open(path, "r", encoding="utf-8") as file:
        content = file.read()

    if not content.strip():
        print("ℹ️ Diff boş, inceleme atlandı.")
        sys.exit(0)

    return content


def call_gemini(api_key: str, system_prompt: str, git_diff: str) -> dict[str, Any]:
    # Kota limitlerine takılmamak için denenecek modeller sırasıyla
    # gemini-1.5-flash-8b genellikle en yüksek kotaya sahiptir.
    models_to_try = ["gemini-2.0-flash", "gemini-1.5-flash", "gemini-1.5-flash-8b", "gemini-1.5-pro"]
    
    payload = {
        "contents": [{"parts": [{"text": f"İncelenecek unified diff:\n\n{git_diff}"}]}],
        "systemInstruction": {"parts": [{"text": system_prompt}]},
        "generationConfig": {
            "temperature": 0.1,
            "responseMimeType": "application/json",
        },
    }

    last_error = None
    
    for model_name in models_to_try:
        print(f"🤖 {model_name} modeli ile inceleme deneniyor...")
        url = (
            f"https://generativelanguage.googleapis.com/v1beta/models/"
            f"{model_name}:generateContent?key={api_key}"
        )
        
        delays = [5, 10] # Her model için 429 durumunda kısa retry
        for idx, delay in enumerate(delays, start=1):
            try:
                response = requests.post(url, json=payload, timeout=120)
                if response.status_code == 200:
                    data = response.json()
                    raw = data["candidates"][0]["content"]["parts"][0]["text"].strip()
                    print(f"✅ {model_name} ile başarılı sonuç alındı.")
                    return json.loads(raw)
                
                if response.status_code == 429:
                    print(f"⚠️ {model_name} Kota Aşımı (429).")
                    last_error = f"Quota Exceeded (429) for {model_name}"
                    if idx < len(delays):
                        print(f"   {delay} saniye bekleniyor...")
                        time.sleep(delay)
                        continue
                    else:
                        print(f"   {model_name} için denemeler bitti, sıradaki modele geçiliyor.")
                        break # Sıradaki modele geç

                if response.status_code == 404:
                    print(f"⚠️ {model_name} modeli bulunamadı (404). Sıradaki modele geçiliyor.")
                    last_error = f"Model {model_name} not found (404)"
                    break

                last_error = f"{response.status_code} - {response.text}"
                print(f"⚠️ {model_name} hatası: {last_error}. Sıradaki modele geçiliyor.")
                break # Diğer hatalarda sıradaki modele geçmeyi dene
            except Exception as ex:
                last_error = str(ex)
                print(f"⚠️ {model_name} beklenmedik hata: {last_error}")
                break

    if any(err in str(last_error) for err in ["429", "Quota", "404", "not found"]):
        print(f"ℹ️ Uygun bir Gemini modeli bulunamadı veya kotalar dolmuş durumda. Build'i bloklamamak için inceleme atlanıyor.")
        return {"summary": "Gemini modelleri erişilemez veya kota aşımı nedeniyle inceleme yapılamadı.", "comments": []}

    print(f"❌ Gemini çağrısı başarısız: {last_error}")
    sys.exit(1)


def get_pr_head_sha(api_url: str, repo: str, pr_number: str, headers: dict[str, str]) -> str:
    url = f"{api_url}/repos/{repo}/pulls/{pr_number}"
    response = requests.get(url, headers=headers, timeout=30)
    if response.status_code != 200:
        print(f"❌ PR bilgisi alınamadı: {response.status_code} - {response.text}")
        sys.exit(1)
    return response.json()["head"]["sha"]


def post_inline_comment(
    api_url: str,
    repo: str,
    pr_number: str,
    headers: dict[str, str],
    commit_id: str,
    file_path: str,
    line: int,
    body: str,
) -> bool:
    url = f"{api_url}/repos/{repo}/pulls/{pr_number}/comments"
    payload = {
        "body": body,
        "commit_id": commit_id,
        "path": file_path,
        "line": line,
        "side": "RIGHT",
    }
    response = requests.post(url, headers=headers, json=payload, timeout=30)
    if response.status_code == 201:
        return True
    print(f"⚠️ Satır yorumu eklenemedi ({file_path}:{line}): {response.status_code}")
    return False


def post_commit_comment(
    api_url: str,
    repo: str,
    commit_sha: str,
    headers: dict[str, str],
    file_path: str,
    line: int,
    body: str,
) -> bool:
    url = f"{api_url}/repos/{repo}/commits/{commit_sha}/comments"
    payload = {
        "body": body,
        "path": file_path,
        "line": line,
    }
    response = requests.post(url, headers=headers, json=payload, timeout=30)
    if response.status_code == 201:
        return True
    print(f"⚠️ Commit yorumu eklenemedi ({file_path}:{line}): {response.status_code} - {response.text}")
    return False


def post_pr_summary(api_url: str, repo: str, pr_number: str, headers: dict[str, str], body: str) -> None:
    url = f"{api_url}/repos/{repo}/issues/{pr_number}/comments"
    response = requests.post(url, headers=headers, json={"body": body}, timeout=30)
    if response.status_code != 201:
        print(f"⚠️ PR özeti eklenemedi: {response.status_code} - {response.text}")


def post_commit_summary(api_url: str, repo: str, commit_sha: str, headers: dict[str, str], body: str) -> None:
    url = f"{api_url}/repos/{repo}/commits/{commit_sha}/comments"
    response = requests.post(url, headers=headers, json={"body": body}, timeout=30)
    if response.status_code != 201:
        print(f"⚠️ Commit özeti eklenemedi: {response.status_code} - {response.text}")


def main() -> None:
    ai_api_key = get_env_or_exit("AI_API_KEY")
    github_token = get_env_or_exit("GITHUB_TOKEN")
    pr_number = os.getenv("PR_NUMBER", "")
    repository = get_env_or_exit("REPOSITORY")
    github_api_url = os.getenv("GITHUB_API_URL", "https://api.github.com")
    event_name = os.getenv("EVENT_NAME", "push")
    commit_sha = os.getenv("COMMIT_SHA", "")

    git_diff = read_diff("pr_diff.txt")
    max_comments = 15

    system_prompt = f"""Sen kıdemli bir kod gözden geçirme uzmanısın. Verilen unified git diff'i incele ve SADECE şu şemada bir JSON nesnesi döndür:
{{
  "summary": "değişikliğin kısa genel özeti (Türkçe)",
  "comments": [
    {{
      "file": "diff'te göründüğü gibi dosya yolu (yeni dosya yolu)",
      "line": 1,
      "severity": "info|minor|major|critical",
      "title": "kısa başlık (Türkçe)",
      "body": "ayrıntılı, uygulanabilir yorum (Türkçe, markdown)"
    }}
  ]
}}
Kurallar:
- TÜM çıktı (summary, title, body) TÜRKÇE olmalı.
- Sadece gerçek sorunları işaretle: doğruluk bug'ları, güvenlik açıkları, race condition'lar, kaynak sızıntıları, bariz performans tuzakları, sınırlarda eksik hata yönetimi, bozuk tipler.
- Gerçek bir sorun yaratmadıkça stil/biçim nit'leri YAZMA.
- `line` mutlaka dosyanın YENİ versiyonunda var olan bir satıra denk gelmeli.
- En fazla {max_comments} yorum döndür. Önemli bir şey yoksa comments boş olabilir.
- Severity değerleri İngilizce kalmalı: info, minor, major, critical.
- Sadece JSON döndür. JSON dışında hiçbir metin yazma."""

    print("🤖 Gemini incelemesi başlatıldı...")
    review = call_gemini(ai_api_key, system_prompt, git_diff)

    summary = str(review.get("summary", "Kod incelemesi tamamlandı."))
    comments = review.get("comments", [])
    if not isinstance(comments, list):
        print("❌ Model geçersiz yorum formatı döndürdü.")
        sys.exit(1)

    gh_headers = {
        "Authorization": f"Bearer {github_token}",
        "Accept": "application/vnd.github+json",
        "X-GitHub-Api-Version": "2022-11-28",
    }

    severity_map = {
        "critical": "🚨 KRİTİK",
        "major": "💥 BÜYÜK HATA",
        "minor": "⚠️ UYARI",
        "info": "ℹ️ BİLGİ",
    }
    blocking = False
    posted_count = 0

    is_pr = event_name == "pull_request" and pr_number
    target_commit_id = ""
    if is_pr:
        target_commit_id = get_pr_head_sha(github_api_url, repository, pr_number, gh_headers)
    else:
        target_commit_id = commit_sha

    for item in comments[:max_comments]:
        file_path = item.get("file")
        line = item.get("line")
        severity = str(item.get("severity", "info")).lower()
        title = str(item.get("title", "İnceleme Notu"))
        body = str(item.get("body", "")).strip()

        if not file_path or not isinstance(line, int):
            continue

        comment_body = f"### {severity_map.get(severity, 'ℹ️ BİLGİ')}: {title}\n\n{body}"
        
        added = False
        if is_pr:
            added = post_inline_comment(
                github_api_url,
                repository,
                pr_number,
                gh_headers,
                target_commit_id,
                str(file_path),
                line,
                comment_body,
            )
        elif target_commit_id:
            added = post_commit_comment(
                github_api_url,
                repository,
                target_commit_id,
                gh_headers,
                str(file_path),
                line,
                comment_body,
            )

        if added:
            posted_count += 1

        if severity in ("major", "critical"):
            blocking = True

    result_text = (
        f"## AI Code Review Özeti\n\n"
        f"**Özet:** {summary}\n\n"
        f"- Eklenen yorum: {posted_count}\n"
        f"- Bloklayıcı hata (major/critical): {'Evet' if blocking else 'Hayır'}\n"
    )

    if is_pr:
        post_pr_summary(github_api_url, repository, pr_number, gh_headers, result_text)
    elif target_commit_id:
        post_commit_summary(github_api_url, repository, target_commit_id, gh_headers, result_text)

    print(f"🤖 Özet: {summary}")
    print(f"📊 Yorum Sayısı: {posted_count}")

    if blocking:
        print("❌ Bloklayıcı hata tespit edildi, workflow fail ediliyor.")
        sys.exit(1)

    print("✅ Bloklayıcı hata yok, workflow başarılı.")
    sys.exit(0)


if __name__ == "__main__":
    main()

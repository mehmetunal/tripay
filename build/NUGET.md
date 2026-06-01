# TriPay NuGet yayınlama

**Sürüm:** `build/TriPay.NuGet.Version.props` içindeki `TriPayPackageVersion` (tek kaynak).

Her `./build/pack-and-push.sh` (push) çalıştırmasında **patch** otomatik artar (`1.0.0` → `1.0.1`).  
`--pack-only` sürümü değiştirmez. Aynı sürümle push: `--no-bump`.  
Minor/major: `TRI_PAY_VERSION_BUMP=minor ./build/pack-and-push.sh`

**Hedef çerçeveler:** `net8.0`, `net9.0`, `net10.0` (tüm kütüphane ve meta paketler). Uygulamanız .NET 8 veya üzeri olmalıdır.

## Paketler (Kullanım Tipine Göre)

Kullanım kılavuzundaki entegrasyon modelleriyle uyumlu hale getirilmiştir:

| Paket | Kullanım Tipi | İçerik |
| :--- | :--- | :--- |
| **TriPay** | **Framework (Mod A)** | `AddTriPayFramework` — Kendi uygulamanız, TriPay DB yok (**Önerilen**). Core, Services, Infrastructure ve Persistence birleşimidir. |
| **TriPay.Hosted** | **Hosted (Mod C)** | `AddTriPayHosted` — MSSQL + checkout + operatör paneli. TriPay paketine ek olarak Data (DB) katmanını içerir. |

> **Not:** `TriPay.Core`, `TriPay.Data`, `TriPay.Infrastructure` ve `TriPay.Persistence` artık bağımsız paketler olarak yayınlanmamaktadır; ana paketlerin içine gömülmüştür.

## Tüketici örneği

```bash
dotnet add package TriPay --version 1.0.0
```

```csharp
// Program.cs
builder.Services.AddTriPayFramework(builder.Configuration);
```

## Yerel pack

```bash
chmod +x build/pack-and-push.sh
./build/pack-and-push.sh --pack-only
```

Çıktı: `artifacts/nupkgs/`

Her pakette:
- Kök **README.md** (nuget.org’da görünür) — web, GitHub, kılavuz linkleri  
- **docs/** klasörü — `TriPay_Kullanim_Kilavuzu.md`, `TriPay_Program_cs_ve_DI.md`, mod rehberleri, `INDEX.md`

## GitHub Actions

Repository secret: **`NUGET_API_KEY`** (nuget.org API anahtarı).

| Workflow | Ne zaman? | Ne yapar? |
| :--- | :--- | :--- |
| **CI** → `nuget-pack` job | `main` push | Sürüm +1 → **pack** → **nuget.org push** → sürüm commit |
| **CI** → `nuget-pack` job | Pull request | Yalnızca pack doğrulama (**push yok**) |
| **NuGet Publish (manuel)** | Actions → Run workflow | İsteğe bağlı patch/minor/major |

## Yerel nuget.org push

```bash
export NUGET_API_KEY=...
./build/pack-and-push.sh
```

veya `/Users/mehmet/Project/maggsoft/nuget-api-key.txt` (tek satır, repoya eklemeyin).

## Yayın sırası

1. **TriPay** (Framework)
2. **TriPay.Hosted** (Hosted)

Script bu sırayı kullanır.

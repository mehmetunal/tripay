#!/usr/bin/env bash
# TriPay git hook'larını .git/hooks altına kurar (pre-commit + pre-push).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SRC="$ROOT/scripts/git-hooks"
DST="$ROOT/.git/hooks"

if [[ ! -d "$DST" ]]; then
  echo "Hata: .git/hooks bulunamadı. Bu komutu depo kökünde çalıştırın." >&2
  exit 1
fi

for hook in pre-commit pre-push; do
  install -m 0755 "$SRC/$hook" "$DST/$hook"
  echo "Kuruldu: .git/hooks/$hook"
done

echo ""
echo "TriPay git hook'ları aktif. Commit ve push öncesi testler zorunlu."
echo "Geçici atlama (önerilmez): SKIP_TESTS=1 git commit ... / git push ..."

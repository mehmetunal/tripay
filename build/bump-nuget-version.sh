#!/usr/bin/env bash
# TriPay.NuGet.Version.props içindeki patch sürümünü bir artırır (1.0.0 → 1.0.1).
# TRI_PAY_VERSION_BUMP=minor|major ile farklı artış (varsayılan: patch).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROPS="${ROOT}/build/TriPay.NuGet.Version.props"
BUMP_KIND="${TRI_PAY_VERSION_BUMP:-patch}"

if [[ ! -f "$PROPS" ]]; then
  echo "Hata: $PROPS bulunamadı." >&2
  exit 1
fi

current="$(sed -n 's/.*<TriPayPackageVersion>\([^<]*\)<\/TriPayPackageVersion>.*/\1/p' "$PROPS" | head -n1 | tr -d '[:space:]')"

if [[ ! "$current" =~ ^([0-9]+)\.([0-9]+)\.([0-9]+)$ ]]; then
  echo "Hata: Geçersiz sürüm '$current' (beklenen: major.minor.patch)" >&2
  exit 1
fi

major="${BASH_REMATCH[1]}"
minor="${BASH_REMATCH[2]}"
patch="${BASH_REMATCH[3]}"

case "$BUMP_KIND" in
  major)
    major=$((major + 1))
    minor=0
    patch=0
    ;;
  minor)
    minor=$((minor + 1))
    patch=0
    ;;
  patch)
    patch=$((patch + 1))
    ;;
  *)
    echo "Hata: TRI_PAY_VERSION_BUMP=$BUMP_KIND (patch|minor|major)" >&2
    exit 1
    ;;
esac

new_version="${major}.${minor}.${patch}"

if [[ "${1:-}" == "--print-only" ]]; then
  echo "$new_version"
  exit 0
fi

# macOS / Linux sed
if sed --version 2>/dev/null | grep -q GNU; then
  sed -i "s|<TriPayPackageVersion>.*</TriPayPackageVersion>|<TriPayPackageVersion>${new_version}</TriPayPackageVersion>|" "$PROPS"
else
  sed -i '' "s|<TriPayPackageVersion>.*</TriPayPackageVersion>|<TriPayPackageVersion>${new_version}</TriPayPackageVersion>|" "$PROPS"
fi

echo "$new_version"

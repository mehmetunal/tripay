#!/usr/bin/env bash
# TriPay NuGet paketlerini derler ve nuget.org'a gönderir.
# Her push öncesi patch sürümü otomatik artar (build/TriPay.NuGet.Version.props).
# API anahtarı: NUGET_API_KEY veya /Users/mehmet/Project/maggsoft/nuget-api-key.txt
#
# Bayraklar:
#   --pack-only   Sürüm artırmadan pack (PR doğrulama)
#   --no-push     Sürüm + pack, push yok (CI pack adımı)
#   --push-only   artifacts/nupkgs içindeki paketleri nuget.org'a gönder (CI push adımı)
#   --no-bump     Sürüm artırma
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="${ROOT}/artifacts/nupkgs"
CONFIG="${NUGET_CONFIG:-${ROOT}/nuget.config}"
VERSION_PROPS="${ROOT}/build/TriPay.NuGet.Version.props"

PACK_ONLY=false
NO_PUSH=false
PUSH_ONLY=false
NO_BUMP=false
for arg in "$@"; do
  case "$arg" in
    --pack-only) PACK_ONLY=true ;;
    --no-push) NO_PUSH=true ;;
    --push-only) PUSH_ONLY=true ;;
    --no-bump) NO_BUMP=true ;;
  esac
done

KEY_FILE="${NUGET_API_KEY_FILE:-/Users/mehmet/Project/maggsoft/nuget-api-key.txt}"
if [[ -z "${NUGET_API_KEY:-}" && -f "$KEY_FILE" ]]; then
  NUGET_API_KEY="$(tr -d '[:space:]' < "$KEY_FILE")"
fi

needs_api_key=false
if [[ "$PUSH_ONLY" == true || ( "$PACK_ONLY" != true && "$NO_PUSH" != true ) ]]; then
  needs_api_key=true
fi
if [[ "$needs_api_key" == true && -z "${NUGET_API_KEY:-}" ]]; then
  echo "Hata: push için NUGET_API_KEY veya $KEY_FILE gerekli." >&2
  exit 1
fi

read_current_version() {
  sed -n 's/.*<TriPayPackageVersion>\([^<]*\)<\/TriPayPackageVersion>.*/\1/p' "$VERSION_PROPS" | head -n1 | tr -d '[:space:]'
}

push_packages() {
  echo ""
  echo "=== nuget.org push (v$(read_current_version)) ==="
  shopt -s nullglob
  nupkgs=("$OUT"/*.nupkg)
  if [[ ${#nupkgs[@]} -eq 0 ]]; then
    echo "Hata: push için .nupkg bulunamadı ($OUT)." >&2
    exit 1
  fi
  for nupkg in "${nupkgs[@]}"; do
    echo ">>> push $(basename "$nupkg")"
    dotnet nuget push "$nupkg" \
      --source https://api.nuget.org/v3/index.json \
      --api-key "$NUGET_API_KEY" \
      --skip-duplicate
    sleep 2
  done
  echo ""
  echo "Push tamamlandı. Sürüm: $(read_current_version)"
}

if [[ "$PUSH_ONLY" == true ]]; then
  push_packages
  exit 0
fi

if [[ "$PACK_ONLY" == true ]]; then
  echo "Sürüm (--pack-only, artırılmadı): $(read_current_version)"
elif [[ "$NO_BUMP" == true ]]; then
  echo "Sürüm (--no-bump): $(read_current_version)"
else
  old="$(read_current_version)"
  new="$(bash "${ROOT}/build/bump-nuget-version.sh")"
  echo "Sürüm: $old → $new (otomatik $(echo "${TRI_PAY_VERSION_BUMP:-patch}"))"
fi

LIBRARIES=(
  "TriPay.Core/TriPay.Core.csproj"
  "TriPay.Services/TriPay.Services.csproj"
  "TriPay.Data/TriPay.Data.csproj"
  "TriPay.Infrastructure/TriPay.Infrastructure.csproj"
  "TriPay.Persistence/TriPay.Persistence.csproj"
)
META=(
  "build/nuget/TriPay.Framework/TriPay.Framework.csproj"
  "build/nuget/TriPay.Hosted/TriPay.Hosted.csproj"
)

cd "$ROOT"
mkdir -p "$OUT"
rm -f "$OUT"/*.nupkg "$OUT"/*.snupkg 2>/dev/null || true

echo ""
echo "=== TriPay NuGet pack — kütüphaneler (v$(read_current_version)) ==="
for proj in "${LIBRARIES[@]}"; do
  echo ">>> pack $proj"
  dotnet pack "$proj" -c Release -o "$OUT" --configfile "$CONFIG" /p:ContinuousIntegrationBuild=true
done

echo "=== TriPay NuGet pack — meta paketler ==="
for proj in "${META[@]}"; do
  echo ">>> pack $proj"
  dotnet pack "$proj" -c Release -o "$OUT" \
    --configfile "${ROOT}/build/nuget/nuget.config" \
    /p:ContinuousIntegrationBuild=true
done

echo ""
echo "Paketler: $OUT"
ls -la "$OUT"

if [[ "$PACK_ONLY" == true || "$NO_PUSH" == true ]]; then
  echo "(--pack-only / --no-push: push atlandı)"
  exit 0
fi

push_packages
echo "Sonraki push: patch otomatik artacak (build/TriPay.NuGet.Version.props)."

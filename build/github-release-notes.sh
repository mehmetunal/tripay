#!/usr/bin/env bash
# GitHub Release gövdesi — NuGet kurulum talimatları ve sürüm linkleri.
# Kullanım: ./build/github-release-notes.sh <sürüm> [önceki-etiket]
# Örnek:   ./build/github-release-notes.sh 1.0.6 v1.0.5
set -euo pipefail

ver="${1:?Sürüm gerekli (ör. 1.0.6)}"
prev_tag="${2:-}"

if [[ -z "$prev_tag" ]]; then
  if [[ "$ver" =~ ^([0-9]+)\.([0-9]+)\.([0-9]+)$ ]]; then
    major="${BASH_REMATCH[1]}"
    minor="${BASH_REMATCH[2]}"
    patch="${BASH_REMATCH[3]}"
    if [[ "$patch" -gt 0 ]]; then
      prev_tag="v${major}.${minor}.$((patch - 1))"
    fi
  fi
fi

cat <<EOF
## NuGet Kurulumu

Paketler [nuget.org](https://www.nuget.org/) üzerinde yayınlanır. Hedef çerçeveler: **net8.0**, **net9.0**, **net10.0**.

### TriPay — Framework modu (Mod A, önerilen)

Kendi uygulamanızda sanal POS entegrasyonu; TriPay MSSQL gerektirmez.

\`\`\`bash
dotnet add package TriPay --version ${ver}
\`\`\`

\`\`\`xml
<PackageReference Include="TriPay" Version="${ver}" />
\`\`\`

\`\`\`csharp
// Program.cs
builder.Services.AddTriPayFramework(builder.Configuration);
\`\`\`

📦 [TriPay ${ver}](https://www.nuget.org/packages/TriPay/${ver})

---

### TriPay.Hosted — Hosted modu (Mod C)

MSSQL + checkout + operatör paneli; \`TriPay\` paketine ek katmanlar içerir.

\`\`\`bash
dotnet add package TriPay.Hosted --version ${ver}
\`\`\`

\`\`\`xml
<PackageReference Include="TriPay.Hosted" Version="${ver}" />
\`\`\`

\`\`\`csharp
// Program.cs
builder.Services.AddTriPayHosted(builder.Configuration);
\`\`\`

📦 [TriPay.Hosted ${ver}](https://www.nuget.org/packages/TriPay.Hosted/${ver})

---

📖 Dokümantasyon: [tripay.com.tr/docs](https://tripay.com.tr/docs) · [Kullanım kılavuzu](https://github.com/mehmetunal/tripay/blob/main/docs/TriPay_Kullanim_Kilavuzu.md)
EOF

if [[ -n "$prev_tag" ]]; then
  cat <<EOF

## Değişiklikler

Tam commit listesi: [${prev_tag}...v${ver}](https://github.com/mehmetunal/tripay/compare/${prev_tag}...v${ver})
EOF
fi

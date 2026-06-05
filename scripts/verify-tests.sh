#!/usr/bin/env bash
# TriPay — tüm unit/integration testlerini çalıştırır; başarısızsa sıfır dışı kod döner.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

echo "TriPay: testler çalıştırılıyor..."
dotnet test TriPay.Tests/TriPay.Tests.csproj --verbosity minimal

echo "TriPay: tüm testler geçti."

# TriPay Test Rehberi

> **Zorunlu kural (Kural #13):** Her davranış değişikliği veya yeni işlem sonrası ilgili **unit** ve gerekiyorsa **integration** testi yazılır ve `dotnet test` ile doğrulanır.

## Proje yapısı

```text
TriPay.Tests/
├── Fixtures/          # Sahte HTTP, gateway ayarları, XML örnekleri, DI fabrikası
├── Unit/              # Hızlı, dış bağımlılık yok (banka ağı mock)
│   ├── Common/
│   ├── Idempotency/
│   ├── Security/
│   ├── Iyzico/
│   ├── Vakifbank/
│   └── VakifPays/
└── Integration/       # DI, idempotency, Auth3DS, WebApplicationFactory
```

## Komutlar

```bash
# Tüm testler
dotnet test
# veya
./scripts/verify-tests.sh

# Solution derleme = build + otomatik test (TriPay.Tests sonunda; başarısız test build'i düşürür)
dotnet build TriPay.sln

# Test çalıştırmadan derleme
dotnet build -p:RunTestsOnBuild=false

# Yalnızca unit
dotnet test --filter "Category!=Integration"

# Yalnızca integration
dotnet test --filter "Category=Integration"

# Tek sınıf
dotnet test --filter "FullyQualifiedName~VakifbankGatewayProviderTests"
```

## Commit / push öncesi zorunlu testler

Depoyu klonladıktan sonra bir kez:

```bash
chmod +x scripts/install-git-hooks.sh scripts/verify-tests.sh
./scripts/install-git-hooks.sh
```

Bu komut `pre-commit` ve `pre-push` hook'larını kurar. **Testler geçmeden commit veya push yapılamaz.**

| Hook | Ne zaman | Davranış |
| :--- | :--- | :--- |
| `pre-commit` | `git commit` | `dotnet test` çalıştırır; başarısızsa commit iptal |
| `pre-push` | `git push` | Aynı doğrulama (commit atlanmışsa ikinci bariyer) |

Acil atlama (önerilmez): `SKIP_TESTS=1 git commit ...` veya `SKIP_TESTS=1 git push ...`

## Yeni işlem eklerken checklist

| Adım | Açıklama |
| :---: | :--- |
| 1 | İşlem hangi katmanda? (`Provider`, `PaymentGatewayService`, `Security`, …) |
| 2 | `Unit/{Provider}/` altında en az bir **başarı** ve bir **hata** senaryosu |
| 3 | Callback / Auth3DS ise `Integration/` + idempotency testi |
| 4 | HTTP banka çağrısı varsa `FakeHttpMessageHandler` + `Fixtures/VakifbankTestXml` |
| 5 | `dotnet test` yeşil |

## Fixture kullanımı

| Fixture | Ne zaman |
| :--- | :--- |
| `FakeHttpClientFactory` + `FakeGatewaySettings` | VakıfPayS provider unit |
| `FakeHttpClientFactory` + `VakifbankTestXml` | Vakıfbank MPI/VPOS |
| `InMemoryVakifbankSaleStateStore` | 3D Auth3DS (Redis yok) |
| `TestServiceProviderFactory` | `IPaymentGatewayService` integration |

## İşlem → test eşlemesi (MVP)

| İşlem | Unit test sınıfı |
| :--- | :--- |
| Initialize (Vakıfbank) | `VakifbankGatewayProviderTests` |
| Callback | `VakifPaysGatewayProviderTests`, `IyzicoGatewayProviderTests`, `VakifbankGatewayProviderTests` |
| Auth3DS | `VakifbankSaleStateIntegrationTests` |
| Idempotency | `RedisIdempotencyStoreTests`, `PaymentGatewayServiceIntegrationTests` |
| Taksit | `VakifPaysGatewayProviderTests`, `VakifbankGatewayProviderTests` |
| PCI / Webhook | `PciDataMaskerTests`, `WebhookSignatureHelperTests` |
| Health | `TriPayWebHealthIntegrationTests` |

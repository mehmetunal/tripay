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
# Tüm testler (43)
dotnet test

# Solution derleme = build + otomatik test (TriPay.Tests sonunda)
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

# TriPay — Proje Çalışma Dokümanı (pwd)

> **Web:** [https://tripay.com.tr](https://tripay.com.tr)  
> Ana doküman: [docs/TriPay_Proje_Dokumani.md](./docs/TriPay_Proje_Dokumani.md)  
> Güvenlik / Redis: [docs/TriPay_Guvenlik_ve_Altrapi_Dokumani.md](./docs/TriPay_Guvenlik_ve_Altrapi_Dokumani.md)

---

## Solution yapısı (doküman §5.3 ile uyumlu)

```text
TriPay.sln
├── TriPay.Core/                 # Result, Redis, Persistence options
├── TriPay.Services/             # Framework: provider'lar, IPaymentGatewayService (DB yok)
├── TriPay.Persistence/          # AddTriPayFramework / AddTriPayHosted, Checkout
├── TriPay.Data/                 # MSSQL (yalnız Hosted)
├── TriPay.Infrastructure/       # Redis, RabbitMQ, metadata cache
├── TriPay/                      # Demo web — AddTriPayHosted
└── TriPay.Tests/
```

**Bağımlılık yönü:** Web → Infrastructure + Services + Data → Core

---

## Redis altyapısı (tüm proje) ✅

Kayıt: `services.AddTriPayInfrastructure(configuration)` → içinde `AddTriPayRedis()`.

| Bileşen | Konum | Redis anahtarı |
| :--- | :--- | :--- |
| `ITriPayRedisCache` | `Infrastructure/Redis/TriPayRedisCache.cs` | `tripay:` önek (InstanceName) |
| Idempotency | `RedisIdempotencyStore` | `idempotency:{key}` |
| Vakıfbank 3D state | `RedisVakifbankSaleStateStore` | `vakifbank:sale:{orderCode}` |
| Dağıtık kilit | `RedisDistributedLock` | `lock:txn:{id}` |
| Rate limit | `RedisRateLimiter` | `rl:{merchantId}` |
| Dev/test (Redis kapalı) | `InMemoryVakifbankSaleStateStore`, `NullIdempotencyStore` | — |

**Yapılandırma (`TriPay:Redis`):**

| Alan | Açıklama |
| :--- | :--- |
| `Enabled` | `true` → StackExchange.Redis; `false` → bellek içi |
| `Configuration` / `ConnectionStrings:Redis` | Bağlantı dizesi |
| `InstanceName` | Varsayılan `tripay:` |
| `IdempotencyTtlDays` | 7 |
| `SaleStateTtlHours` | 24 |
| `DistributedLockSeconds` | 30 |
| `RateLimitMaxRequests` | 120 / dakika |

**Program.cs:** [TriPay_Program_cs_ve_DI.md](./docs/TriPay_Program_cs_ve_DI.md) — `AddTriPayFramework` veya `AddTriPayHosted` (+ migration)

---

## MVP Sanal POS ✅

| Kanal | Durum |
| :--- | :---: |
| Iyzico | ✅ |
| Vakıfbank | ✅ |
| VakıfPayS | ✅ |

---

## Veritabanı + Outbox ✅

- `Transactions`, `TransactionLogs`, `OutboxMessages`
- `PaymentCheckoutService` — DB tutar doğrulama + Redis kilit
- `OutboxDispatcherHostedService` → RabbitMQ

---

## Testler

```bash
dotnet test                    # 43 test (manuel)
dotnet build TriPay.sln        # Derleme + TriPay.Tests sonrası otomatik test
dotnet build -p:RunTestsOnBuild=false   # Test çalıştırmadan derleme
```

`Directory.Build.targets` — `TriPay.Tests` build edildiğinde `dotnet test --no-build` otomatik çalışır.

Development: `TriPay:Redis:Enabled: false`, `TriPay:Database:UseInMemory: true`

Üretim / Docker: `Redis:Enabled: true` + `docker compose up -d`

---

## Gateway metadata (DB + Redis) ✅

- Tablolar: `GatewaySettings`, `GatewayErrorMappings`
- `IGatewayMetadataService` + `RedisCachedGatewayMetadataService`
- Vakıfbank URL / hata kodları seed: `202605220004`

---

## Program.cs ve DI ✅

**Tek kaynak:** [docs/TriPay_Program_cs_ve_DI.md](./docs/TriPay_Program_cs_ve_DI.md)

| Mod | `Program.cs` | API |
| :--- | :--- | :--- |
| Framework | `AddTriPayFramework` | `IPaymentGatewayService` |
| Hosted | `AddTriPayHosted` + `RunTriPayMigrations` | `IPaymentCheckoutService` |
| Hosted C‑Lite | Aynı + `PersistTransactionLogs: false` | Checkout |

`AddTriPay()` tek başına üretimde kullanılmaz (iç yapı / test).

---

## EN SON FAZ — Admin panel + Identity ⏳

> **Kod yazılmadı.** Tam spesifikasyon: [docs/TriPay_Proje_Dokumani.md §17](./docs/TriPay_Proje_Dokumani.md#17-yönetim-paneli-admin--en-son-faz)

| Öğe | Karar |
| :--- | :--- |
| Proje | `TriPay.Admin` (Bootstrap 5 MVC, Türkçe) |
| Giriş | ASP.NET Core Identity |
| Migration | **FluentMigrator** (`AspNetUsers`, roller, …) — EF migration değil |
| Seed admin | `admin@gmail.com` / `Super123!` |
| Öncelik | Ödeme + webhook + `MerchantGateways` bittikten **sonra** |

**Panelde olacaklar (özet):** Dashboard, işlem/log inceleme, merchant listesi, gateway ayarları + hata sözlüğü CRUD (Redis invalidation), outbox kuyruğu, Identity kullanıcı/rol yönetimi.

**Panelde olmayacaklar:** PAN/CVV, appsettings credential düzenleme, merchant self-servis.

---

**Son güncelleme:** 2026-05-22 — §17 Admin + Identity planı dokümana eklendi (implementasyon en son)

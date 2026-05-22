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
├── TriPay.Admin/                # Yönetim paneli — Tailwind + Identity
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

## Admin panel — Tamamlandı ✅ (Tailwind + Gulp)

| Öğe | Durum |
| :--- | :--- |
| Proje | `TriPay.Admin` — **Tailwind CSS 4** (`npm run build:css`) |
| UI | **Tailwind CSS 4** + Gulp; JS: Trimango `core/*` + `ModuleFactory` + **tam AJAX** (`ApiService`, `UIService`) |
| Identity | FluentMigrator `202605220010_IdentitySchema` |
| Doğrulama | **FluentValidation** — `TriPay.Admin/Validators/*` (DataAnnotations yalnızca `[Display]`) |
| Katmanlar (SOLID) | Controller → `Application/Services` → `TriPay.Data/Repositories/Admin` + `Application/Dtos` |
| Seed (Development) | `admin@gmail.com` / `Super123!` |
| Modüller | Dashboard, İşlemler, **Raporlar**, Merchants, Gateways (ayar+hata CRUD), Outbox (+ yeniden kuyruk), Kullanıcılar (oluştur/şifre/kilit), **Roller / yetkiler**, Sistem |
| Roller | `Admin` (tam yetki, kod) · `User` (DB’de `AspNetRoleClaims`, `permission` claim) |
| Yetki yönetimi | `Roles/Index` → **User** rolü izinleri düzenlenir; **Admin** rolü UI’da kilitli |
| IP kısıtı | `TriPay:Admin:AllowedIpRanges` (`appsettings`) — rol değil, ağ filtresi |

**Rol / yetki (özet):**

- İzin kodları: `TriPay.Data/Identity/AdminPermissions.cs` (`panel.access`, `transactions.view`, `merchants.manage`, …)
- Seed: `AdminPermissionSeeder` — roller + User varsayılan claim’leri
- Oturum: `ApplicationClaimsPrincipalFactory` — User rolündeki permission claim’leri cookie’ye eklenir
- Policy: `AddTriPayAdminAuthorization()` — her izin için ayrı policy; giriş için `panel.access` fallback

```bash
cd TriPay.Admin && npm install && npm run build
# Kaynak: wwwroot/js/**, wwwroot/css/admin.css (Tailwind)
# Minify: wwwroot/js-built/**, wwwroot/css-built/** (Trimango.Web ile ayni; _AdminScripts / _AdminStyles)
dotnet run --project TriPay.Admin   # https://localhost:5055
```

Giriş (Development): `admin@gmail.com` / `Super123!`

---

**Son güncelleme:** 2026-05-22 — Admin rol/yetki (DB) + menü policy filtreleme

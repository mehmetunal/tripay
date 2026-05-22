# TriPay — Program.cs ve DI (tek kaynak)

> **Bu dosya, `Program.cs` ve servis kaydı için tek referanstır.**  
> Diğer dokümanlar buraya link verir; aynı tabloları tekrar etmez.

**Versiyon:** 1.0 · **Tarih:** 22 Mayıs 2026

---

## 1. Önce hangi mod?

| Sizin durumunuz | `Program.cs` satırı | Enjekte edeceğiniz servis |
| :--- | :--- | :--- |
| Kendi siteniz / API; TriPay veritabanı **istemiyorsunuz** (NuGet, KVKK düşük risk) | `AddTriPayFramework(configuration)` | `IPaymentGatewayService` |
| TriPay MSSQL + checkout + operatör paneli (demo web gibi) | `AddTriPayHosted(configuration)` | `IPaymentCheckoutService` |
| Hosted ama ham API logu **istemiyorsunuz** | `AddTriPayHosted(configuration)` + `PersistTransactionLogs: false` | `IPaymentCheckoutService` |
| Sadece unit test / özel DI denemesi | `AddTriPay(configuration)` — **üretimde tek başına kullanmayın** | `IPaymentGatewayService` (eksik parçalar sizde) |

**`AddTriPay()` tek başına üretim girişi değildir.** Framework ve Hosted extension’ları bunu **içeride** çağırır; siz yalnızca üsttekilerden birini seçersiniz.

---

## 2. `AddTriPay()` vs `AddTriPayFramework()` — karşılaştırma

İkisi **aynı şey değildir**. Framework, `AddTriPay`’i **içeride çağıran** üst seviye pakettir.

### Tek cümle

| | Anlam |
| :--- | :--- |
| **`AddTriPay()`** | Motor parçası: banka provider’ları + `IPaymentGatewayService` |
| **`AddTriPayFramework()`** | Üretim paketi: `AddTriPay` + Redis + banka ayarları (`appsettings`); TriPay DB yok |

### Program.cs — ne yazarsınız?

```csharp
// ❌ Üretim — eksik kalır (Redis, IGatewaySettingsProvider yok)
builder.Services.AddTriPay(builder.Configuration);

// ✅ Üye işyeri / NuGet — önerilen
builder.Services.AddTriPayFramework(builder.Configuration);
```

### Kayıt karşılaştırması (kodla uyumlu)

| Özellik | `AddTriPay()` | `AddTriPayFramework()` |
| :--- | :---: | :---: |
| **Sizin `Program.cs`’te çağrı** | Test / özel DI | **Üretim (Framework mod)** |
| **Proje** | `TriPay.Services` | `TriPay.Persistence` (içinde `AddTriPay` çağırır) |
| `VakifPays` / `Iyzico` / `Vakifbank` provider | Evet | Evet (AddTriPay üzerinden) |
| `IPaymentGatewayService` | Evet | Evet |
| `AddHttpClient()` | Evet | Evet (AddTriPay üzerinden) |
| `TriPayOptions` / `PersistenceOptions` config bind | Evet (config verilirse) | Evet |
| **`IGatewaySettingsProvider`** (banka credential okuma) | **Hayır** | Evet — `ConfigurationGatewaySettingsProvider` (`appsettings`) |
| **`AddTriPayRedis()`** (3D state, idempotency, kilit) | **Hayır** | Evet |
| `TriPayDbContext` / MSSQL | Hayır | Hayır |
| `IPaymentCheckoutService` | Hayır | Hayır (Persistence zorla kapalı) |
| `TransactionLogs` | Hayır | Hayır |
| `RunTriPayMigrations()` gerekir mi? | Hayır | Hayır |

### İç çağrı zinciri

```text
AddTriPayFramework(configuration)
  ├── AddTriPay(configuration)          ← provider'lar + IPaymentGatewayService
  ├── AddTriPayRedis(configuration)     ← Redis / bellek içi cache
  ├── IGatewaySettingsProvider          ← TriPay:Gateways:*:Settings
  └── PersistenceOptions → Enabled=false, log/outbox kapalı
```

```text
AddTriPay(configuration)   ← tek başına — zincirin sadece ilk halkası
  ├── Configure TriPayOptions
  ├── AddHttpClient()
  ├── PaymentGatewayFactory + 3 provider
  └── IPaymentGatewayService
```

### Davranış farkı (pratik)

| Durum | Yalnız `AddTriPay()` | `AddTriPayFramework()` |
| :--- | :--- | :--- |
| VakıfPayS / Iyzico ödeme başlat | Credential `IGatewaySettingsProvider` olmadan **patlayabilir** | `appsettings`’ten ayar okunur |
| Vakıfbank 3D (MPI → satış) | Redis state store **yok** | `AddTriPayRedis` ile state saklanır |
| İşlem TriPay DB’ye yazılır mı? | Hayır | Hayır — sipariş **sizin** DB’nizde |
| KVKK (TriPay tarafı) | Provider çalışırsa bankaya gider; TriPay log/DB yok | Aynı + Redis TTL ile geçici 3D veri |

### Üç kullanım — yan yana

| | `AddTriPay()` | `AddTriPayFramework()` | `AddTriPayHosted()` |
| :--- | :--- | :--- | :--- |
| **Kim yazar?** | Test / framework geliştirici | **Üye işyeri** | **Operatör / demo web** |
| **TriPay MSSQL** | Hayır | Hayır | Evet |
| **Ana servis** | `IPaymentGatewayService` | `IPaymentGatewayService` | `IPaymentCheckoutService` |
| **Banka ayarı kaynağı** | Siz eklemelisiniz | `appsettings` | `appsettings` + DB metadata |
| **Normal log (`TransactionLogs`)** | Hayır | Hayır | Config (`true` / `false`) |

Detaylı Hosted: [TriPay_Hosted_Modu.md](./TriPay_Hosted_Modu.md)

---

## 3. Extension hiyerarşisi (kafanız karışmasın)

```text
ÜRETİM — sizin yazdığınız tek satır:

  AddTriPayFramework()     veya     AddTriPayHosted()


İÇ YAPI — genelde Program.cs'te ÇAĞIRMAYIN:

  AddTriPayHosted içinde sırayla:
    AddTriPayData → AddTriPayInfrastructure → AddTriPay → AddTriPayPersistence

  AddTriPayFramework içinde sırayla:
    AddTriPay → AddTriPayRedis → IGatewaySettingsProvider (appsettings)
```

| Extension | Kim çağırır? | Ne kaydeder? |
| :--- | :--- | :--- |
| `AddTriPay()` | Framework / Hosted (içeride) | Provider’lar + `IPaymentGatewayService` |
| `AddTriPayFramework()` | **Siz** (üye işyeri) | `AddTriPay` + Redis + appsettings ayarları; **DB yok** |
| `AddTriPayHosted()` | **Siz** (operatör / demo) | Data + Infrastructure + `AddTriPay` + Persistence |
| `AddTriPayData()` | Sadece Hosted (içeride) | MSSQL + FluentMigrator |
| `AddTriPayInfrastructure()` | Sadece Hosted (içeride) | Redis, RabbitMQ, gateway metadata DB |
| `AddTriPayPersistence()` | Sadece Hosted (içeride) | `IPaymentCheckoutService` |

---

## 4. ASP.NET Core Web — Framework (önerilen)

**Proje:** Kendi MVC / API uygulamanız. **TriPay MSSQL yok.**

```csharp
using TriPay.Persistence.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(); // veya AddControllers
builder.Services.AddTriPayFramework(builder.Configuration);

var app = builder.Build();
// RunTriPayMigrations() YOK — TriPay DB yok

app.MapControllers();
app.Run();
```

**Controller:**

```csharp
public class PaymentController : Controller
{
    private readonly IPaymentGatewayService _payment;

    public PaymentController(IPaymentGatewayService payment) => _payment = payment;
}
```

**`appsettings.json` (özet):**

```json
{
  "TriPay": {
    "Persistence": {
      "Enabled": false,
      "PersistTransactionLogs": false,
      "EnableOutbox": false
    },
    "Redis": { "Enabled": true, "Configuration": "localhost:6379" },
    "Gateways": {
      "VakifPays": {
        "Enabled": true,
        "IsTestMode": true,
        "Settings": { "Merchant": "...", "MerchantUser": "...", "MerchantPassword": "..." }
      }
    }
  }
}
```

Detay (KVKK, API örnekleri): [TriPay_Framework_Modu.md](./TriPay_Framework_Modu.md)

---

## 5. ASP.NET Core Web — Hosted (demo / operatör)

**Proje:** `TriPay` demo web veya operatör host. **MSSQL + checkout.**

```csharp
using TriPay.Data.DependencyInjection;
using TriPay.Persistence.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddTriPayHosted(builder.Configuration);

var app = builder.Build();
app.Services.RunTriPayMigrations(); // InMemory DB ise atlanır

app.MapControllers();
app.Run();
```

**Controller:** `IPaymentCheckoutService` (işlem DB’ye yazılır).

```csharp
public class CheckoutController : Controller
{
    private readonly IPaymentCheckoutService _checkout;

    public CheckoutController(IPaymentCheckoutService checkout) => _checkout = checkout;
}
```

**Hosted tam (normal log — `TransactionLogs`):**

```json
"TriPay": {
  "Persistence": {
    "Enabled": true,
    "PersistTransactionLogs": true,
    "EnableOutbox": true
  }
}
```

**Hosted C‑Lite (log kapalı):** Aynı `AddTriPayHosted()`; yalnızca `"PersistTransactionLogs": false`.

Detay (tablolar, health): [TriPay_Hosted_Modu.md](./TriPay_Hosted_Modu.md)

**Canlı referans:** `TriPay/Program.cs` (repo).

---

## 6. Console / Worker — Framework

Eski kılavuzdaki `Host.CreateDefaultBuilder` + yalnız `AddTriPay()` **eksik kayıt** bırakır (Redis, gateway settings yok). Console’da da **Framework** kullanın:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TriPay.Persistence.DependencyInjection;
using TriPay.Services.Interfaces;
using TriPay.Services.Models;
using TriPay.Services.Providers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddTriPayFramework(context.Configuration);
    })
    .Build();

var payment = host.Services.GetRequiredService<IPaymentGatewayService>();

var result = await payment.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
{
    GatewayName = PaymentGatewayNames.VakifPays,
    Payment = new PaymentRequest
    {
        Amount = 100m,
        OrderNumber = "TEST-001",
        ReturnUrl = "https://localhost/callback",
        TestPlatform = true
    }
});
```

> `services.AddHttpClient()` ayrıca yazmanıza gerek yok; `AddTriPayFramework` → `AddTriPay` zaten `AddHttpClient()` çağırır.

---

## 7. `AddTriPay()` ne zaman?

| Senaryo | Doğru kayıt |
| :--- | :--- |
| Üretim web/API | `AddTriPayFramework` veya `AddTriPayHosted` |
| Unit test, minimal provider denemesi | `AddTriPay(configuration)` + test fixture’ları |
| “Sadece `AddTriPay()` yazdım, ödeme çalışmıyor” | Eksik: Redis, `IGatewaySettingsProvider` veya DB — **§2 tablosuna dönün** |

Kısa ek notlar: [TriPay_AddTriPay_Dusuk_Seviye.md](./TriPay_AddTriPay_Dusuk_Seviye.md)

---

## 8. Sık yapılan hatalar

| Hata | Sonuç | Düzeltme |
| :--- | :--- | :--- |
| `AddTriPay()` tek başına (Console/Web) | Ayar/Redis eksik, 3D veya config patlar | `AddTriPayFramework` |
| Framework projede `AddTriPayHosted` | Gereksiz MSSQL bağımlılığı | `AddTriPayFramework` |
| Hosted’de `RunTriPayMigrations` unutulmak | Tablo yok | Migration satırını ekleyin |
| Hosted sanıp `IPaymentGatewayService` inject | Checkout DB yazılmaz | `IPaymentCheckoutService` |
| Kılavuzdaki eski `AddTriPay()` + `AddHttpClient()` örneği | Güncel değil | Bu dosya §6 |
| `AddTriPay` ile `AddTriPayFramework`’ü karıştırmak | Eksik Redis / settings | **§2 karşılaştırma** |

---

## 9. İlgili dokümanlar

| Konu | Dosya |
| :--- | :--- |
| API kullanımı (Initialize, Callback, …) | [TriPay_Kullanim_Kilavuzu.md](./TriPay_Kullanim_Kilavuzu.md) |
| Framework iş kuralları | [TriPay_Framework_Modu.md](./TriPay_Framework_Modu.md) |
| Hosted + DB + C‑Lite | [TriPay_Hosted_Modu.md](./TriPay_Hosted_Modu.md) |
| Kapsam / KVKK | [TriPay_Kapsam_ve_Entegrasyon_Modelleri.md](./TriPay_Kapsam_ve_Entegrasyon_Modelleri.md) |

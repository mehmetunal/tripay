# TriPay.Framework

**TriPay** — [Maggsoft](https://maggsoft.com.tr) alt ürünü · Türkiye ödeme entegrasyon katmanı

**Desteklenen çerçeveler:** .NET 8, .NET 9, .NET 10 ve üzeri (`net8.0`, `net9.0`, `net10.0`)

## Bağlantılar

- **Web:** https://tripay.com.tr  
- **Kullanım kılavuzu:** https://tripay.com.tr/docs  
- **GitHub:** https://github.com/mehmetunal/tripay  
- **E-posta:** info@tripay.com.tr  

## Ne zaman kullanılır?

Kendi web siteniz veya API; banka bilgileri `appsettings` / Vault; **TriPay MSSQL zorunlu değil** (KVKK dostu).

## Kurulum

```bash
dotnet add package TriPay.Framework --version 1.0.0
```

## Program.cs

```csharp
using TriPay.Persistence.DependencyInjection;

builder.Services.AddTriPayFramework(builder.Configuration);
```

## Örnek — ödeme başlatma

```csharp
using TriPay.Services.Interfaces;
using TriPay.Services.Models;
using TriPay.Services.Providers;
using TriPay.Services.Providers.VakifPays.Models;

var result = await payment.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
{
    GatewayName = PaymentGatewayNames.VakifPays,
    Payment = new PaymentRequest
    {
        OrderNumber = "SIP-001",
        Amount = 1500.00m,
        Currency = "TRY",
        Use3D = true,
        TestPlatform = true,
        ReturnUrl = "https://magaza.com/payment/callback",
        // kart ve müşteri alanları...
    }
});

if (result.IsSuccess && !string.IsNullOrEmpty(result.Data?.RedirectHtml))
    return Content(result.Data.RedirectHtml, "text/html");
```

## Paket içi dokümantasyon

Bu NuGet paketinin `docs/` klasöründe tam markdown seti vardır. Önce:

1. **docs/TriPay_Kullanim_Kilavuzu.md** — kullanım kılavuzu (request/response, callback)  
2. **docs/TriPay_Program_cs_ve_DI.md** — DI ve `Program.cs`  
3. **docs/TriPay_Framework_Modu.md** — yapılandırma  

Dizin: **docs/INDEX.md**

## Bağımlılıklar

`TriPay`, `TriPay.Infrastructure`, `TriPay.Persistence` (meta paket ile birlikte gelir).

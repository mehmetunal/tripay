# TriPay (Framework Modu)

**TriPay** — [Maggsoft](https://maggsoft.com.tr) alt ürünü · Türkiye ödeme entegrasyon katmanı

Bu paket, TriPay'in **Framework Modu (Mod A)** için ana kütüphanedir. Kendi uygulamanızda banka bilgilerini yönetmek ve TriPay veritabanı (MSSQL) kullanmadan ödeme almak için tasarlanmıştır.

## Paket ve DLL Yapısı

Bu NuGet paketi aşağıdaki DLL dosyalarını içerir:

| DLL Dosyası | Görevi |
| :--- | :--- |
| **`TriPay.Services.dll`** | Ana ödeme motoru ve banka provider'ları. |
| **`TriPay.Core.dll`** | Ortak modeller, interface'ler ve sabitler. |
| **`TriPay.Infrastructure.dll`** | Redis, Idempotency ve HTTP altyapısı. |

## Bağlantılar

- **Web:** https://tripay.com.tr  
- **Kullanım kılavuzu:** https://tripay.com.tr/docs  
- **GitHub:** https://github.com/mehmetunal/tripay  

## Kurulum

```bash
dotnet add package TriPay
```

## Program.cs

```csharp
using TriPay.Services.DependencyInjection;

// Framework modu (TriPay DB yok — önerilen)
builder.Services.AddTriPayFramework(builder.Configuration);
```

## Örnek — Ödeme Başlatma

```csharp
using TriPay.Services.Interfaces;
using TriPay.Services.Models;
using TriPay.Core.Gateways;

var result = await payment.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
{
    GatewayName = PaymentGatewayNames.VakifPays,
    Payment = new PaymentRequest
    {
        OrderNumber = "SIP-001",
        Amount = 1500.00m,
        ReturnUrl = "https://magaza.com/payment/callback"
    }
});
```

## Paket içi dokümantasyon

Bu NuGet paketinin `docs/` klasöründe tam markdown seti vardır:
1. **docs/TriPay_Kullanim_Kilavuzu.md** — API rehberi
2. **docs/TriPay_Program_cs_ve_DI.md** — DI ve Program.cs
3. **docs/TriPay_Framework_Modu.md** — Yapılandırma

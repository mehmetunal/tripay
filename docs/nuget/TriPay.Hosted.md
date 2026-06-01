# TriPay.Hosted

**TriPay** — [Maggsoft](https://maggsoft.com.tr) alt ürünü

## Bağlantılar

- **Web:** https://tripay.com.tr  
- **Kullanım kılavuzu:** https://tripay.com.tr/docs  
- **GitHub:** https://github.com/mehmetunal/tripay  
- **E-posta:** info@tripay.com.tr  

## Ne zaman kullanılır?

TriPay MSSQL, hosted checkout (`/pay`), operatör paneli ve merkezi işlem kaydı.

## Kurulum

```bash
dotnet add package TriPay.Hosted --version 1.0.0
```

## Program.cs

```csharp
using TriPay.Persistence.DependencyInjection;

builder.Services.AddTriPayHosted(builder.Configuration);
```

## Servis

`IPaymentCheckoutService.PayAsync(PaymentRequest, gatewayName)`

## Paket içi dokümantasyon

| Dosya | Konu |
| :--- | :--- |
| docs/TriPay_Kullanim_Kilavuzu.md | API rehberi |
| docs/TriPay_Hosted_Modu.md | Hosted kurulum |
| docs/TriPay_Program_cs_ve_DI.md | DI kayıtları |
| docs/TriPay_Admin_ve_Veritabani.md | Veritabanı şeması |

Tam liste: **docs/INDEX.md**

# TriPay.Hosted (Hosted Modu)

**TriPay** — [Maggsoft](https://maggsoft.com.tr) alt ürünü · Türkiye ödeme entegrasyon katmanı

Bu paket, TriPay'in **Hosted Modu (Mod C)** için ana kütüphanedir. TriPay MSSQL veritabanı, checkout servisi (`/pay`) ve operatör paneli desteği içerir.

## Paket ve DLL Yapısı

Bu NuGet paketi aşağıdaki DLL dosyalarını içerir (bağımlılık olarak `TriPay` paketini de yükler):

| DLL Dosyası | Kaynak Paket | Görevi |
| :--- | :--- | :--- |
| **`TriPay.Hosted.dll`** | `TriPay.Hosted` | Hosted modu DI kayıtları ve giriş noktası. |
| **`TriPay.Persistence.dll`** | `TriPay.Hosted` | EF Core DbContext ve `PaymentCheckoutService`. |
| **`TriPay.Data.dll`** | `TriPay.Hosted` | Veritabanı varlıkları ve DTO'lar. |
| **`TriPay.Services.dll`** | `TriPay` (Bağımlılık) | Ana ödeme motoru ve banka provider'ları. |
| **`TriPay.Core.dll`** | `TriPay` (Bağımlılık) | Ortak modeller ve interface'ler. |
| **`TriPay.Infrastructure.dll`** | `TriPay` (Bağımlılık) | Redis ve altyapı servisleri. |

## Bağlantılar

- **Web:** https://tripay.com.tr  
- **Kullanım kılavuzu:** https://tripay.com.tr/docs  
- **GitHub:** https://github.com/mehmetunal/tripay  

## Kurulum

```bash
dotnet add package TriPay.Hosted
```

## Program.cs

```csharp
using TriPay.Persistence.DependencyInjection;

// Hosted modu (TriPay MSSQL + checkout)
builder.Services.AddTriPayHosted(builder.Configuration);

// Veritabanı göçlerini çalıştır (opsiyonel)
app.Services.RunTriPayMigrations();
```

## Paket içi dokümantasyon

| Dosya | Konu |
| :--- | :--- |
| docs/TriPay_Kullanim_Kilavuzu.md | API rehberi |
| docs/TriPay_Hosted_Modu.md | Hosted kurulum |
| docs/TriPay_Program_cs_ve_DI.md | DI kayıtları |
| docs/TriPay_Admin_ve_Veritabani.md | Veritabanı şeması |

Tam liste: **docs/INDEX.md**

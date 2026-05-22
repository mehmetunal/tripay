# TriPay

**Web:** [https://tripay.com.tr](https://tripay.com.tr)  
**Tüm Ödemeler Tek Platformda** — .NET 8+ (8 / 9 / 10) · SQL Server · Payment Hub

## Dokümantasyon

| Doküman | İçerik |
| :--- | :--- |
| [**TriPay_Program_cs_ve_DI.md**](./docs/TriPay_Program_cs_ve_DI.md) | **`Program.cs` tek kaynak** — Framework / Hosted / Console |
| [**TriPay_Kullanim_Kilavuzu.md**](./docs/TriPay_Kullanim_Kilavuzu.md) | API A–Z (Initialize, Callback, …) |
| [**pwd.md**](./pwd.md) | Geliştirici özet + TODO |
| [**TriPay_Proje_Dokumani.md**](./docs/TriPay_Proje_Dokumani.md) | Mimari, POS listesi, kurallar |
| [**TriPay_Framework_Modu.md**](./docs/TriPay_Framework_Modu.md) | Framework appsettings + KVKK |
| [**TriPay_Hosted_Modu.md**](./docs/TriPay_Hosted_Modu.md) | Hosted DB + C‑Lite |
| [**TriPay_Kapsam_ve_Entegrasyon_Modelleri.md**](./docs/TriPay_Kapsam_ve_Entegrasyon_Modelleri.md) | Amaç, risk |
| [**TriPay_Admin_ve_Veritabani.md**](./docs/TriPay_Admin_ve_Veritabani.md) | Hosted tablolar + admin |
| [**TriPay_Admin_Fazlar.md**](./docs/TriPay_Admin_Fazlar.md) | Admin panel faz planı (Tailwind) |
| [**TriPay_Guvenlik_ve_Altrapi_Dokumani.md**](./docs/TriPay_Guvenlik_ve_Altrapi_Dokumani.md) | **Güvenlik, işlem, RabbitMQ, Docker, Kubernetes** |

> **Zorunlu:** Kod yazmadan önce proje dokümanını okuyun. Entegrasyon için kullanım kılavuzunu takip edin.

## Yerel altyapı (Redis + RabbitMQ + MSSQL)

```bash
docker compose up -d
# RabbitMQ UI: http://localhost:15672 (tripay / tripay_dev_only — yalnızca geliştirme)
dotnet run --project TriPay
```

Detay: [TriPay_Guvenlik_ve_Altrapi_Dokumani.md](./docs/TriPay_Guvenlik_ve_Altrapi_Dokumani.md)

## Testler

```bash
dotnet test
```

Yapı ve kurallar: [TriPay_Test_Rehberi.md](./docs/TriPay_Test_Rehberi.md)

## Hızlı başlangıç (NuGet / DLL)

```bash
# NuGet (yayın sonrası)
dotnet add package TriPay --version 1.0.0   # sürüm: build/TriPay.NuGet.props

# Monorepo referansı
dotnet add reference ../tripay/TriPay.Services/TriPay.Services.csproj
```

```csharp
using TriPay.Persistence.DependencyInjection;

// Framework modu (TriPay DB yok — önerilen)
builder.Services.AddTriPayFramework(builder.Configuration);

// Controller — IPaymentGatewayService
var result = await _payment.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
{
    GatewayName = PaymentGatewayNames.VakifPays,
    Payment = paymentRequest
});
```

Detay: [Program.cs ve DI](./docs/TriPay_Program_cs_ve_DI.md) · [Kullanım kılavuzu](./docs/TriPay_Kullanim_Kilavuzu.md)

## Demo uygulama (MVC)

```bash
cd TriPay && dotnet run
```

---

**TriPay** · [tripay.com.tr](https://tripay.com.tr) · v1.0

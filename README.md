# TriPay

**Tüm Ödemeler Tek Platformda** — .NET 10 · SQL Server · Payment Hub

## Dokümantasyon

| Doküman | İçerik |
| :--- | :--- |
| [**pwd.md**](./pwd.md) | **Öncelik TODO:** iyzico → Vakıfbank → VakıfPayS (✅) |
| [**Kılavuz §7**](./docs/TriPay_Kullanim_Kilavuzu.md#74-çoklu-banka-yapılandırması-neden-farklı) | Tüm banka/kuruluş **dış config** örnekleri (`appsettings`) |
| [**TriPay_Proje_Dokumani.md**](./docs/TriPay_Proje_Dokumani.md) | Mimari, POS listesi, veritabanı, kurallar |
| [**TriPay_Kullanim_Kilavuzu.md**](./docs/TriPay_Kullanim_Kilavuzu.md) | **Kullanım kılavuzu A–Z:** NuGet, DLL, HttpClient, kod örnekleri |

> **Zorunlu:** Kod yazmadan önce proje dokümanını okuyun. Entegrasyon için kullanım kılavuzunu takip edin.

## Hızlı başlangıç (NuGet / DLL)

```bash
# NuGet (yayın sonrası)
dotnet add package TriPay --version 1.0.0   # sürüm: build/TriPay.NuGet.props

# Monorepo referansı
dotnet add reference ../tripay/TriPay.Services/TriPay.Services.csproj
```

```csharp
using TriPay.Services;

// Program.cs
builder.Services.AddTriPay();

// Controller
var result = await _payment.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
{
    GatewayName = PaymentGatewayNames.VakifPays,
    Payment = paymentRequest
});
```

Detay: [Kullanım kılavuzu](./docs/TriPay_Kullanim_Kilavuzu.md)

## Demo uygulama (MVC)

```bash
cd TriPay && dotnet run
```

---

**TriPay Geliştirme Ekibi** · v3.0

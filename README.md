# TriPay

**Web:** [https://tripay.com.tr](https://tripay.com.tr)  
**Tüm Ödemeler Tek Platformda** — .NET 8+ (8 / 9 / 10) · SQL Server · Payment Hub

TriPay, Türkiye’deki banka ve ödeme kuruluşu sanal POS kanallarını **tek bir API ve provider mimarisi** altında toplayan ödeme hub’ıdır. Üye işyeri uygulamanız yalnızca `IPaymentGatewayService` ile konuşur; hangi bankanın veya kuruluşun kullanılacağı `GatewayName` ve `TriPay:Gateways` yapılandırması ile belirlenir. Kart verisi, 3D Secure, callback ve iade akışları kanal bazlı provider’larda (`{Kanal}GatewayProvider`) kapsüllenir; iş mantığınız Clean Architecture ve MediatR desenleriyle ayrı kalır.

## Desteklenen kanallar

Aşağıdaki harita, TriPay ekosisteminde hedeflenen **banka** ve **ödeme kuruluşu** sanal POS ağını özetler. Her kanal için `PaymentGatewayNames` sabiti, `PaymentGatewayFactory` kaydı ve `appsettings` config şablonu tanımlıdır.

![Bankalar ve ödeme kuruluşları — TriPay sanal POS ekosistemi](https://raw.githubusercontent.com/mehmetunal/tripay/main/docs/bankalar.png)

| Kategori | Örnek kanallar |
| :--- | :--- |
| **Bankalar** | Akbank, Garanti BBVA, İş Bankası, Halkbank, Ziraat, Yapı Kredi, Denizbank, QNB Finansbank, Kuveyt Türk, Vakıf Katılım, … |
| **Nestpay / EST** | Akbank Nestpay, Finansbank Nestpay, Cardplus, Alternatif Bank, Anadolubank, … |
| **Ödeme kuruluşları** | Iyzico, VakıfPayS, Paratika, Payten MSU, Sipay, QNBpay, ParamPos, Moka, Tami, PayNKolay, Paynet, … |

Tam liste, işlem tipleri (satış, 3D, iptal, iade) ve kod durumu: [TriPay_Proje_Dokumani.md §6](./docs/TriPay_Proje_Dokumani.md#6-olması-gerekenler--kullanılabilir-sanal-poslar).  
Kanal başına `appsettings` şablonları (A–R): [Kullanım Kılavuzu §7.7](./docs/TriPay_Kullanim_Kilavuzu.md#77-config-şablonları-ar).

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
# veya solution build (TriPay.Tests sonunda testler otomatik çalışır)
dotnet build TriPay.sln
```

Commit/push öncesi test zorunluluğu için hook kurulumu:

```bash
./scripts/install-git-hooks.sh
```

Yapı ve kurallar: [TriPay_Test_Rehberi.md](./docs/TriPay_Test_Rehberi.md)

## Test ortamı yapılandırması

Kanalları denemek için `TriPay:Gateways` bloğunu `appsettings.json` veya `appsettings.Development.json` içine ekleyin. Tüm kanallarda `IsTestMode: true` kullanın; test dışı bir kanalı açmak için yalnızca ilgili gateway için `Enabled: true` yapın.

Config şablonları ve alan açıklamaları: [Kullanım Kılavuzu §7.7](./docs/TriPay_Kullanim_Kilavuzu.md#77-config-şablonları-ar).

> **Not:** **Vakıfbank**, **VakıfPayS** ve **Iyzico** ayarları aşağıdaki örnekte `TriPay.Demo/appsettings.json` ile aynı bırakılmıştır; değiştirmeyin.

### Ortak test kartları

| Kanal / platform | Kart numarası | SKT | CVV |
| :--- | :--- | :--- | :--- |
| Garanti BBVA | `5289394722895016` | `01/2025` | `030` |
| İş Bankası | `4508034508034509` | `12/2026` | `000` |
| CCPayment (QNBpay, Sipay, …) | `4022780520669303` | `01/2050` | `988` |
| Nestpay (Asseco genel) | `4355084355084358` | `12/2030` | `000` |

### Nestpay test URL’leri

| Ortam | API | 3D Secure |
| :--- | :--- | :--- |
| Asseco (çoğu Nestpay bankası) | `https://entegrasyon.asseco-see.com.tr/fim/api` | `https://entegrasyon.asseco-see.com.tr/fim/est3Dgate` |
| İş Bankası | `https://istest.asseco-see.com.tr/fim/api` | `https://istest.asseco-see.com.tr/fim/est3Dgate` |
| Ziraat Bankası | `https://torus-stage-ziraat.asseco-see.com.tr/fim/api` | `https://torus-stage-ziraat.asseco-see.com.tr/fim/est3Dgate` |

Nestpay bankaları (Halkbank, Ziraat, Cardplus, …) için mağaza kodu, API kullanıcısı ve 3D store key bilgileri bankanızın **Asseco test panelinden** alınır. Aşağıdaki JSON’da yalnızca **Garanti**, **İş Bankası** ve **CCPayment** grubu için örnek test değerleri doludur.

### Doğrulanmış test kimlik bilgileri

| TriPay kanalı | Settings alanları | Değer |
| :--- | :--- | :--- |
| **Garanti** | `MerchantId` / `TerminalId` / `ProvPassword` / `StoreKey` | `7000679` / `30691297` / `123qweASD/` / `12345678` |
| **IsBankasi** | `MerchantId` / `Username` / `Password` / `StoreKey` | `700655000200` / `ISBANKAPI` / `ISBANK07` / `TRPS0200` |
| **QNBpay**, **Sipay**, **PayBull**, **Parolapara**, **IQmoney**, **Vepara**, **HalkOde** | `AppId` / `AppSecret` / `MerchantKey` | `07fb70f9d8de575f32baa6518e38c5d6` / `61d97b2cac247069495be4b16f8604db` / `$2y$10$N9IJkgazXMUwCzpn7NJrZePy3v.dIFOQUyW4yGfT3eWry6m.KxanK` |

Diğer kanallar (Akbank, Paratika, ParamPos, Moka, Tami, …) için test üye işyeri bilgilerini ilgili banka veya ödeme kuruluşundan alın. Alan adları: [Kullanım Kılavuzu §7.7](./docs/TriPay_Kullanim_Kilavuzu.md#77-config-şablonları-ar).

### Örnek `TriPay:Gateways` (tüm kanallar)

Aşağıdaki bloğu `appsettings.json` içindeki mevcut `TriPay:Gateways` bölümüne birleştirin. Varsayılan olarak yalnızca **VakıfPayS** açıktır; diğer kanalları test etmek için ilgili satırda `Enabled: true` yapın.

```json
{
  "TriPay": {
    "DefaultGateway": "VakifPays",
    "Gateways": {
      "VakifPays": {
        "Enabled": true,
        "IsTestMode": true,
        "Settings": {
          "Merchant": "10009011",
          "MerchantUser": "apitest48@vakifpays.com.tr",
          "MerchantPassword": "Api.123.1234"
        }
      },
      "Iyzico": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "ApiKey": "",
          "SecretKey": ""
        }
      },
      "Vakifbank": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "MerchantPassword": "",
          "TerminalNo": "",
          "InstallmentCounts": "3,6,9",
          "BinPrefixes": ""
        }
      },
      "Garanti": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "7000679",
          "TerminalId": "30691297",
          "ProvPassword": "123qweASD/",
          "StoreKey": "12345678"
        }
      },
      "IsBankasi": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "700655000200",
          "Username": "ISBANKAPI",
          "Password": "ISBANK07",
          "StoreKey": "TRPS0200"
        }
      },
      "Akbank": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "AkbankNestpay": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "AlternatifBank": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "Anadolubank": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "Halkbank": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "ING": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "Sekerbank": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "TurkEkonomiBankasi": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "TurkiyeFinans": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "Ziraat": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "Cardplus": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "FinansbankNestpay": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "Denizbank": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "QNBFinansbank": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "YapiKredi": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "TerminalId": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "KuveytTurk": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "VakifKatilim": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "Username": "",
          "Password": "",
          "StoreKey": ""
        }
      },
      "Paratika": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "MerchantUser": "",
          "MerchantPassword": ""
        }
      },
      "PaytenMsu": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "MerchantUser": "",
          "MerchantPassword": ""
        }
      },
      "ZiraatPay": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "MerchantUser": "",
          "MerchantPassword": ""
        }
      },
      "QNBpay": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "AppId": "07fb70f9d8de575f32baa6518e38c5d6",
          "AppSecret": "61d97b2cac247069495be4b16f8604db",
          "MerchantKey": "$2y$10$N9IJkgazXMUwCzpn7NJrZePy3v.dIFOQUyW4yGfT3eWry6m.KxanK"
        }
      },
      "Sipay": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "AppId": "07fb70f9d8de575f32baa6518e38c5d6",
          "AppSecret": "61d97b2cac247069495be4b16f8604db",
          "MerchantKey": "$2y$10$N9IJkgazXMUwCzpn7NJrZePy3v.dIFOQUyW4yGfT3eWry6m.KxanK"
        }
      },
      "PayBull": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "AppId": "07fb70f9d8de575f32baa6518e38c5d6",
          "AppSecret": "61d97b2cac247069495be4b16f8604db",
          "MerchantKey": "$2y$10$N9IJkgazXMUwCzpn7NJrZePy3v.dIFOQUyW4yGfT3eWry6m.KxanK"
        }
      },
      "Parolapara": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "AppId": "07fb70f9d8de575f32baa6518e38c5d6",
          "AppSecret": "61d97b2cac247069495be4b16f8604db",
          "MerchantKey": "$2y$10$N9IJkgazXMUwCzpn7NJrZePy3v.dIFOQUyW4yGfT3eWry6m.KxanK"
        }
      },
      "IQmoney": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "AppId": "07fb70f9d8de575f32baa6518e38c5d6",
          "AppSecret": "61d97b2cac247069495be4b16f8604db",
          "MerchantKey": "$2y$10$N9IJkgazXMUwCzpn7NJrZePy3v.dIFOQUyW4yGfT3eWry6m.KxanK"
        }
      },
      "Vepara": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "AppId": "07fb70f9d8de575f32baa6518e38c5d6",
          "AppSecret": "61d97b2cac247069495be4b16f8604db",
          "MerchantKey": "$2y$10$N9IJkgazXMUwCzpn7NJrZePy3v.dIFOQUyW4yGfT3eWry6m.KxanK"
        }
      },
      "HalkOde": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "AppId": "07fb70f9d8de575f32baa6518e38c5d6",
          "AppSecret": "61d97b2cac247069495be4b16f8604db",
          "MerchantKey": "$2y$10$N9IJkgazXMUwCzpn7NJrZePy3v.dIFOQUyW4yGfT3eWry6m.KxanK"
        }
      },
      "ParamPos": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "ClientCode": "",
          "ClientUsername": "",
          "ClientPassword": "",
          "Guid": ""
        }
      },
      "Ahlpay": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "MerchantUser": "",
          "MerchantPassword": "",
          "StoreKey": ""
        }
      },
      "Moka": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "DealerCode": "",
          "Username": "",
          "Password": ""
        }
      },
      "Tami": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "MerchantUser": "",
          "MerchantPassword": "",
          "StoreKey": ""
        }
      },
      "PayNKolay": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "",
          "StoreKey": ""
        }
      },
      "Paynet": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantPassword": ""
        }
      }
    }
  }
}
```

**Hızlı test:** Demo uygulamada `TriPay.Demo/appsettings.json` dosyasını düzenleyip test etmek istediğiniz kanal için `Enabled: true` ve `DefaultGateway` değerini güncelleyin; ardından `dotnet run --project TriPay`.

## Hızlı başlangıç (NuGet)

```bash
# Framework modu (önerilen — kendi uygulamanız, TriPay DB yok)
dotnet add package TriPay --version 1.0.0

# Hosted modu (TriPay MSSQL + checkout)
dotnet add package TriPay.Hosted --version 1.0.0
```

```csharp
using TriPay.Services.DependencyInjection;

// Framework modu (TriPay DB yok — önerilen)
builder.Services.AddTriPayFramework(builder.Configuration);

// Controller — IPaymentGatewayService
var result = await _payment.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
{
    GatewayName = PaymentGatewayNames.VakifPays,
    Payment = paymentRequest
});
```

Detay: [Program.cs ve DI](./docs/TriPay_Program_cs_ve_DI.md) · [Kullanım kılavuzu](./docs/TriPay_Kullanim_Kilavuzu.md) · [NuGet yayınlama](./build/NUGET.md)

**TriPay**, [Maggsoft](https://maggsoft.com.tr) alt ürünüdür.

## Demo uygulama (MVC)

```bash
cd TriPay && dotnet run
```

---

**TriPay** · [tripay.com.tr](https://tripay.com.tr) · v1.0

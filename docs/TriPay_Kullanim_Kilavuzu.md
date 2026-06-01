> **Dosya Adı:** `TriPay_Kullanim_Kilavuzu.md`  
> **İlişkili:** [TriPay_Proje_Dokumani.md](./TriPay_Proje_Dokumani.md) (mimari ve kurallar)

# TriPay Kullanım Kılavuzu (A–Z)

**Versiyon:** 1.1 · **Tarih:** 22 Mayıs 2026 · **Web:** [https://tripay.com.tr](https://tripay.com.tr)

Bu kılavuz, TriPay’in **tüm entegrasyon seçeneklerini** kapsar: Framework (NuGet), Hosted, HttpClient API ve yapılandırma modları.


> **`Program.cs` ve DI için tek kaynak:** [**TriPay_Program_cs_ve_DI.md**](./TriPay_Program_cs_ve_DI.md) — mod seçimi, **`AddTriPay` vs `AddTriPayFramework` karşılaştırması (§2)**, tam kod örnekleri.

| Ek doküman | Ne zaman okunur? |
| :--- | :--- |
| **[TriPay_Program_cs_ve_DI.md](./TriPay_Program_cs_ve_DI.md)** | **İlk okuyun** — `AddTriPayFramework` / `AddTriPayHosted` / `AddTriPay` |
| [TriPay_Framework_Modu.md](./TriPay_Framework_Modu.md) | Framework: appsettings, API, KVKK |
| [TriPay_Hosted_Modu.md](./TriPay_Hosted_Modu.md) | Hosted: MSSQL tabloları, C‑Lite |
| [TriPay_AddTriPay_Dusuk_Seviye.md](./TriPay_AddTriPay_Dusuk_Seviye.md) | Yalnız test / iç yapı (`AddTriPay`) |
| [TriPay_Kapsam_ve_Entegrasyon_Modelleri.md](./TriPay_Kapsam_ve_Entegrasyon_Modelleri.md) | Amaç, risk |
| [TriPay_Admin_ve_Veritabani.md](./TriPay_Admin_ve_Veritabani.md) | Hosted admin + DB |

---

## İçindekiler

1. [TriPay nedir?](#1-tripay-nedir)
2. [Entegrasyon modelleri](#2-entegrasyon-modelleri) — özet; **Program.cs:** [TriPay_Program_cs_ve_DI.md](./TriPay_Program_cs_ve_DI.md)
3. [Gereksinimler](#3-gereksinimler)
4. [Paket ve DLL yapısı](#4-paket-ve-dll-yapısı)
5. [NuGet ile kurulum](#5-nuget-ile-kurulum)
6. [Doğrudan DLL referansı](#6-doğrudan-dll-referansı)
7. [DI kaydı ve yapılandırma](#7-di-kaydı-ve-yapılandırma) — **Program.cs:** [TriPay_Program_cs_ve_DI.md](./TriPay_Program_cs_ve_DI.md)
  - [7.2–7.8 Banka config / Redis şablonları](#72-yapılandırma-appsettingsjson--hızlı-örnek-tek-kanal)
8. [Provider (banka) seçimi](#8-provider-banka-seçimi)
9. [Temel kavramlar ve modeller](#9-temel-kavramlar-ve-modeller)
10. [Ödeme başlatma (Initialize)](#10-ödeme-başlatma-initialize)
11. [3D Secure akışı](#11-3d-secure-akışı)
12. [Callback işleme](#12-callback-işleme)
13. [Taksit sorgulama](#13-taksit-sorgulama)
14. [Ödeme durumu sorgulama](#14-ödeme-durumu-sorgulama)
15. [İade (Refund)](#15-i̇ade-refund)
16. [Aktif gateway listesi](#16-aktif-gateway-listesi)
17. [HttpClient ile kullanım (uzak API)](#17-httpclient-ile-kullanım-uzak-api)
18. [ASP.NET Core MVC entegrasyonu](#18-aspnet-core-mvc-entegrasyonu)
19. [Console / Worker Service örneği](#19-console--worker-service-örneği)
20. [Hata yönetimi](#20-hata-yönetimi)
21. [Güvenlik ve PCI](#21-güvenlik-ve-pci)
22. [Test ortamı](#22-test-ortamı)
23. [Sık sorulan sorular](#23-sık-sorulan-sorular)
24. [Hızlı referans tablosu](#24-hızlı-referans-tablosu)

---

## 1. TriPay nedir?

**Resmi web sitesi:** [https://tripay.com.tr](https://tripay.com.tr)

TriPay; banka ve ödeme kuruluşu sanal POS’larını tek arayüzde birleştiren bir **Payment Hub** kütüphanesidir. Entegrasyon developer’ı:

- Hangi **provider**’ları kullanacağını belirler (`GatewayName`, `MerchantGateways` — bkz. proje dokümanı §5.5),
- Ödeme, callback, taksit, iade işlemlerini `**IPaymentGatewayService`** üzerinden yapar.

**Şu an kodda aktif kanal:** `VakifPays`  
**Hedef:** §6’daki tüm kanallar (iyzico, Garanti, PayTR, …)

---

## 2. Entegrasyon modelleri (tüm seçenekler)

> **Kapsam ve KVKK:** [TriPay_Kapsam_ve_Entegrasyon_Modelleri.md](./TriPay_Kapsam_ve_Entegrasyon_Modelleri.md)  
> **Admin / MSSQL tabloları:** [TriPay_Admin_ve_Veritabani.md](./TriPay_Admin_ve_Veritabani.md)

### 2.0. Hızlı seçim tablosu

> **Program.cs kodu ve extension açıklaması:** [TriPay_Program_cs_ve_DI.md](./TriPay_Program_cs_ve_DI.md)


| Kod        | Mod                  | NuGet Paketi         | TriPay MSSQL | DI girişi                                             | Ana API                   |
| ---------- | -------------------- | -------------------- | ------------ | ----------------------------------------------------- | ------------------------- |
| **A**      | Framework (NuGet)    | `TriPay`             | Hayır        | `AddTriPayFramework()`                                | `IPaymentGatewayService`  |
| **A+**     | Framework + Redis    | `TriPay`             | Hayır        | `AddTriPayFramework()`                                | Aynı                      |
| **C**      | Hosted tam           | `TriPay.Hosted`      | Evet         | `AddTriPayHosted()`                                   | `IPaymentCheckoutService` |
| **C‑Lite** | Hosted, log kapalı   | `TriPay.Hosted`      | Evet (özet)  | `AddTriPayHosted()` + `PersistTransactionLogs: false` | Checkout                  |
| **B**      | HttpClient API       | — (REST)             | Sizde yok    | HTTP client                                           | REST (planlanan)          |
| **D**      | Hosted ödeme sayfası | `TriPay.Hosted`      | Evet         | TriPay.Web URL                                        | Tarayıcı redirect         |


**En düşük risk (sizin isteğiniz):** Mod **A** — banka bilgisi sizde, TriPay işlem/log tutmaz.

---

### 2.1. Mod A — Framework (NuGet / DLL) — önerilen

> **NuGet Paketi:** `dotnet add package TriPay`  
> **Tam doküman:** [TriPay_Framework_Modu.md](./TriPay_Framework_Modu.md)

Üye işyeri kendi .NET uygulamasında TriPay kütüphanesini çağırır. Banka credential ve sipariş kaydı **tamamen sizin** sisteminizdedir.

```text
Framework  →  AddTriPayFramework()  →  IPaymentGatewayService
             →  TriPay MSSQL YOK  →  TransactionLogs YOK
```

```mermaid
flowchart TB
    subgraph Merchant["Üye işyeri"]
        App[ASP.NET / API / Worker]
        CFG[appsettings / Key Vault<br/>Gateway Settings]
        MDB[(Sizin veritabanınız<br/>sipariş + ödeme durumu)]
    end
    subgraph TriPayFW["TriPay Framework"]
        AddPay[AddTriPayFramework]
        Svc[IPaymentGatewayService]
        Prov[VakifPays / Iyzico / Vakifbank Provider]
    end
    Bank[Banka / iyzico API]

    App --> AddPay
    CFG --> Prov
    App --> Svc --> Prov --> Bank
    App --> MDB
```



**Program.cs:** `AddTriPayFramework(builder.Configuration)` — tam örnek: [TriPay_Program_cs_ve_DI.md §4](./TriPay_Program_cs_ve_DI.md#4-aspnet-core-web--framework-önerilen)

**appsettings (örnek):**

```json
{
  "TriPay": {
    "Persistence": {
      "Enabled": false,
      "PersistTransactionLogs": false,
      "EnableOutbox": false
    },
    "Gateways": {
      "VakifPays": {
        "Enabled": true,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "SIZIN_ID",
          "MerchantPassword": "SIZIN_SIFRE",
          "TerminalNo": "SIZIN_TERMINAL"
        }
      }
    }
  }
}
```

**Ödeme çağrısı:**

```csharp
var result = await _payment.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
{
    GatewayName = PaymentGatewayNames.VakifPays,
    Payment = paymentRequest
});
// Sonucu kendi DB'nize siz yazarsınız
```


| Dahil                                            | Hariç                             |
| ------------------------------------------------ | --------------------------------- |
| Provider adaptörleri, 3D, callback, taksit, iade | `Transactions`, `TransactionLogs` |
| `appsettings` gateway config                     | TriPay `Merchants` tablosu        |
| Opsiyonel Redis (3D state)                       | `IPaymentCheckoutService`         |


---

### 2.2. Mod A+ — Framework + Redis (3D / idempotency)

Mod A ile aynı; ek olarak Redis:

- Vakıfbank 3D `sale` state
- Idempotency, rate limit (ileri faz)

```mermaid
flowchart LR
    App[Uygulama] --> Svc[IPaymentGatewayService]
    Svc --> Redis[(Redis TTL)]
    Svc --> Bank[Banka API]
```



`AddTriPayFramework` zaten `AddTriPayRedis` çağırır. `TriPay:Redis:Enabled: true` yeterli.

---

### 2.3. Mod C — Hosted (TriPay operatörü / demo web)

> **NuGet Paketi:** `dotnet add package TriPay.Hosted`  
> **Tam doküman:** [TriPay_Hosted_Modu.md](./TriPay_Hosted_Modu.md) — Bölüm 1 (Hosted Tam)

```text
Hosted  →  AddTriPayHosted()  →  IPaymentCheckoutService + MSSQL
        →  PersistTransactionLogs: true  →  TransactionLogs (normal log alanı)
```

TriPay MSSQL’de işlem özeti; maskeli API logları (`TransactionLogs`); webhook outbox.

```mermaid
flowchart TB
    subgraph Host["TriPay Hosted"]
        Web[TriPay.Web MVC]
        Hosted[AddTriPayHosted]
        Checkout[IPaymentCheckoutService]
        TriDB[(TriPay MSSQL)]
        Meta[GatewaySettings + ErrorMappings]
        Redis[(Redis cache)]
        RMQ[RabbitMQ Outbox]
    end
    Bank[Banka API]

    Web --> Checkout --> TriDB
    Hosted --> Meta --> Redis
    Checkout --> Bank
    TriDB --> RMQ
```



**Program.cs:** `AddTriPayHosted` + `RunTriPayMigrations()` — tam örnek: [TriPay_Program_cs_ve_DI.md §5](./TriPay_Program_cs_ve_DI.md#5-aspnet-core-web--hosted-demo--operatör)

**appsettings:**

```json
"TriPay": {
  "Persistence": {
    "Enabled": true,
    "PersistTransactionLogs": true,
    "EnableOutbox": true
  }
}
```

---

### 2.4. Mod C‑Lite — Hosted, KVKK hafif profil

> **NuGet Paketi:** `dotnet add package TriPay.Hosted`  
> **Tam doküman:** [TriPay_Hosted_Modu.md](./TriPay_Hosted_Modu.md) — Bölüm 2 (Hosted C‑Lite)

```text
Hosted C-Lite  →  AddTriPayHosted()  →  IPaymentCheckoutService + MSSQL
               →  PersistTransactionLogs: false  →  TransactionLogs YAZILMAZ
               →  Transactions özeti kalır
```

İşlem özeti TriPay’de (`Transactions`); **ham banka logu yok** (`TransactionLogs` kapalı).

```json
"Persistence": {
  "Enabled": true,
  "PersistTransactionLogs": false,
  "EnableOutbox": true
}
```

```mermaid
flowchart LR
    Pay[PayAsync] --> Txn[Transactions tablosu]
    Pay -.->|kapalı| Log[TransactionLogs]
    Txn --> Outbox[OutboxMessages]
```



---

### 2.5. Mod B — HttpClient (uzak TriPay REST API)

Uygulamanızda TriPay DLL **yok**; yalnızca HTTP. Veritabanı **TriPay API sunucusunda**.

```mermaid
sequenceDiagram
    participant App as Sizin uygulama
    participant API as TriPay REST API
    participant Bank as Banka

    App->>API: POST /api/payments/initialize
    Note over App: Authorization: Bearer merchant_api_key
    API->>Bank: Provider HTTP
    Bank-->>API: 3D HTML / sonuç
    API-->>App: JSON cevap
```



> **Durum:** REST uçları planlanmıştır. Geçici olarak Mod **A** (in-process) veya Mod **C** (hosted sayfa) kullanın. Hedef sözleşme: [§17 HttpClient](#17-httpclient-ile-kullanım-uzak-api).

---

### 2.6. Mod D — Hosted ödeme sayfası (redirect)

Kullanıcı `https://tripay.com.tr/Checkout` benzeri sayfaya yönlendirilir; PCI yükü TriPay host’ta azalır.

```mermaid
flowchart LR
    Shop[E-ticaret] -->|redirect| Page[TriPay.Web Checkout]
    Page --> Hosted[AddTriPayHosted]
    Hosted --> Bank[Banka 3D]
    Bank -->|callback| Page
    Page -->|webhook| Shop
```



---

### 2.7. DI özeti

Üretimde yalnızca **`AddTriPayFramework`** veya **`AddTriPayHosted`** yazın. Alt extension’lar (`AddTriPay`, `AddTriPayData`, …) iç yapıdır — tablo ve hatalar: [TriPay_Program_cs_ve_DI.md](./TriPay_Program_cs_ve_DI.md).


**Monorepo paketleri:**

```text
TriPay.Services      → NuGet çekirdek (DB yok)
TriPay.Persistence   → Hosted checkout (opsiyonel referans)
TriPay.Data          → MSSQL
TriPay.Infrastructure→ Redis, RabbitMQ, metadata cache
```

---

## 3. Gereksinimler


| Gereksinim | Değer                                                                                             |
| ---------- | ------------------------------------------------------------------------------------------------- |
| .NET       | **8.0 ve üzeri** — NuGet paketi `net8.0`, `net9.0`, `net10.0` hedefler (8 / 9 / 10 / sonraki LTS) |
| Proje tipi | ASP.NET Core Web, Web API, Worker, Console                                                        |
| NuGet      | `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Http`                           |
| Opsiyonel  | MSSQL — yalnız **Hosted (Model C)**; Framework (Model A) için **gerekmez**                        |


---

## 4. Paket ve DLL yapısı

NuGet paket yapısı:


| Paket / DLL                         | İçerik                                                      |
| ----------------------------------- | ----------------------------------------------------------- |
| `**TriPay`** (NuGet `PackageId`)    | Framework — `TriPay.Services` (TriPay.Data **dahil değil**) |
| `**TriPay.Persistence`** (monorepo) | Hosted — `AddTriPayHosted`, `PaymentCheckoutService`        |
| `**TriPay.Data**` (monorepo)        | MSSQL, FluentMigrator — Hosted ile                          |


> **Neden `TriPay`?** Kısa marka adı; entegrasyon `dotnet add package TriPay` ile tek satır. Derleme çıktısı DLL adı `TriPay.Services.dll` olabilir — bu normaldir (`PackageId` ≠ assembly adı).

**Derleme çıktısı (yerel geliştirme):**

```text
TriPay.Services/bin/Release/net8.0/   (veya net9.0 / net10.0 — proje TFM’ine göre)
├── TriPay.Services.dll          ← ana kütüphane
├── TriPay.Services.deps.json
└── (bağımlılıklar)
```

Kütüphanenin public yüzeyi:


| Tip                                          | Açıklama                                             |
| -------------------------------------------- | ---------------------------------------------------- |
| `IPaymentGatewayService`                     | Tüm ödeme işlemleri (Facade)                         |
| `PaymentGatewayFactory`                      | Provider çözümleme                                   |
| `PaymentGatewayNames`                        | Gateway kod sabitleri (`const`) — magic string yasak |
| `PaymentGateway*RequestDto` / `*ResponseDto` | İstek/cevap modelleri                                |
| `PaymentRequest`                             | Kart ve sipariş bilgisi                              |
| `Result<T>`                                  | Standart sonuç sarmalayıcı                           |
| `AddTriPay()`                                | DI extension                                         |


**Kod düzeni (zorunlu — proje dokümanı Kural #14):**

- Her `public` tip (**class**, `record`, `struct`, `enum`) **kendi `.cs` dosyasında** tanımlanır.
- `PaymentGatewayModels.cs` gibi çoklu sınıf dosyaları **kullanılmaz**; örnek: `Models/PaymentGatewayInitializeRequestDto.cs`.
- Provider’a özel modeller ilgili provider klasöründe: `Providers/VakifPays/Models/SaleResponse.cs`.

---

## 5. NuGet ile kurulum

**Merkezi yapılandırma (repo):** `build/TriPay.NuGet.props` (`PackageId`, `Version`), `Directory.Packages.props` (bağımlılık sürümleri), `Directory.Build.props` (ortak framework + import).

### 5.1. dotnet CLI (yayın sonrası)

```bash
dotnet add package TriPay --version 1.0.0
```

> Paket adı: `**TriPay**` (kısa ve tek marka). İçerik `TriPay.Services` projesinden üretilir. Henüz nuget.org’da yoksa [§6 Doğrudan DLL](#6-doğrudan-dll-referansı) veya `ProjectReference` kullanın.

### 5.2. `.csproj` referansı

```xml
<ItemGroup>
  <PackageReference Include="TriPay" Version="1.0.0" />
</ItemGroup>
```

### 5.3. Paket restore ve doğrulama

```bash
dotnet restore
dotnet build
```

Başarılı build sonrası `IPaymentGatewayService` IntelliSense’te görünür.

---

## 6. Doğrudan DLL referansı

NuGet yerine derlenmiş **.dll** ile referans (iç ağ, CI artifact, lokal geliştirme):

### 6.1. DLL’yi kopyala

```text
lib/TriPay/
├── TriPay.Services.dll
└── TriPay.Services.xml          (varsa — IntelliSense)
```

### 6.2. Proje referansı

```xml
<ItemGroup>
  <Reference Include="TriPay.Services">
    <HintPath>..\lib\TriPay\TriPay.Services.dll</HintPath>
  </Reference>
</ItemGroup>
```

veya **ProjectReference** (monorepo / aynı solution):

```xml
<ItemGroup>
  <ProjectReference Include="..\tripay\TriPay.Services\TriPay.Services.csproj" />
</ItemGroup>
```

### 6.3. Bağımlılıklar

`TriPay.Services` şunlara ihtiyaç duyar:

- `Microsoft.AspNetCore.App` (framework reference)
- `Newtonsoft.Json`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Http`
- `Microsoft.Extensions.Logging.Abstractions`

ASP.NET Core Web projesinde çoğu zaten vardır.

---

## 7. DI kaydı ve yapılandırma

**`Program.cs`, mod seçimi ve tam kod örnekleri bu kılavuzda tekrarlanmaz.** Tek kaynak: [**TriPay_Program_cs_ve_DI.md**](./TriPay_Program_cs_ve_DI.md).

Bu bölümde yalnızca **banka `appsettings` şeması**, Redis ve config şablonları vardır.

### 7.1. Constructor injection

```csharp
using TriPay.Services.Interfaces;

public class PaymentController : Controller
{
    private readonly IPaymentGatewayService _payment;

    public PaymentController(IPaymentGatewayService payment)
    {
        _payment = payment;
    }
}
```

### 7.2. Yapılandırma (`appsettings.json`) — hızlı örnek (tek kanal)

Geliştirme için yalnızca **VakıfPayS** (mevcut kod):

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
          "MerchantUser": "api@firma.com",
          "MerchantPassword": "***"
        }
      }
    }
  }
}
```

> **Not:** VakıfPayS, Iyzico/Vakıfbank ile aynı şekilde `HttpPaymentGatewayBase` + `TriPay:Gateways:VakifPays:Settings` kullanır (`Merchant`, `MerchantUser`, `MerchantPassword`).

---

### 7.3. Çoklu banka yapılandırması (neden farklı?)

Türkiye'de her sanal POS / ödeme kuruluşu **farklı kimlik bilgisi** ve **farklı API sözleşmesi** kullanır:


| Grup                         | Tipik alanlar                                                                | Örnek kanallar                               |
| ---------------------------- | ---------------------------------------------------------------------------- | -------------------------------------------- |
| **A — REST API Key**         | `ApiKey`, `SecretKey`, `IsTestMode`                                          | Iyzico, Sipay, ParamPos, Paynet, Vepara, …   |
| **B — Nestpay / EST 3D**     | `MerchantId`, `TerminalId`, `Username`, `Password`, `StoreKey`               | Akbank, İş Bankası, Halkbank, Ziraat, YKB, … |
| **C — Garanti PROV**         | `MerchantId`, `TerminalId`, `ProvUserId`, `ProvPassword`, `StoreKey`         | Garanti BBVA                                 |
| **D — Vakıfbank MPI + VPOS** | `MerchantId`, `MerchantPassword`, `TerminalNo`, URL'ler, `InstallmentCounts` | Vakıfbank                                    |
| **E — VakıfPayS REST**       | `Merchant`, `MerchantUser`, `MerchantPassword`                               | VakıfPayS                                    |
| **F — PayTR**                | `MerchantId`, `MerchantKey`, `MerchantSalt`                                  | PayTR                                        |


TriPay'de **tek tip dış config** hedeflenir; provider içinde kanala özel alanlar `Settings` sözlüğünden okunur:

```text
Sizin uygulama
    └── TriPay:Gateways:{PaymentGatewayNames.*}
            └── Settings: { "ApiKey": "...", "MerchantId": "..." }
                    └── IyzicoGatewayProvider / VakifbankGatewayProvider / …
```

**Önemli:** Ödeme isteğinde yalnızca `**GatewayName`** (hangi kanal) gönderilir; **API key / şifre istek gövdesinde taşınmaz.**

---

### 7.4. Dışarıdan config verme (üç katman)


| Katman                                  | Ne zaman                 | Nasıl                                       |
| --------------------------------------- | ------------------------ | ------------------------------------------- |
| **1 — `appsettings` / ortam değişkeni** | Geliştirme, tek merchant | `TriPay:Gateways:{Kod}:Settings`            |
| **2 — `MerchantGateways` (MSSQL)**      | Üretim, çok üye işyeri   | Şifreli credential JSON (proje dokümanı §9) |
| **3 — Key Vault / secret store**        | Üretim                   | `Settings` vault'tan doldurulur             |


**Ortam değişkeni örneği:**

```bash
export TriPay__Gateways__Iyzico__Settings__ApiKey="sandbox-xxx"
export TriPay__Gateways__Iyzico__Settings__SecretKey="sandbox-yyy"
export TriPay__Gateways__Iyzico__IsTestMode="true"
```

**C# bağlama (hedef — provider port sonrası):**

```csharp
builder.Services.AddTriPayFramework(builder.Configuration);
// veya Hosted: AddTriPayHosted(builder.Configuration)
```

**Üye işyeri paneli (hedef — `MerchantGateways`):**

```json
{
  "gatewayCode": "Vakifbank",
  "isEnabled": true,
  "isDefault": false,
  "settings": {
    "MerchantId": "000000000000001",
    "MerchantPassword": "***",
    "TerminalNo": "VP000001"
  }
}
```

---

### 7.5. Merkezi `appsettings` şeması (tüm kanallar)

Anahtar = `PaymentGatewayNames` değeri (`"Iyzico"`, `"Vakifbank"`, …).

```json
{
  "TriPay": {
    "DefaultGateway": "VakifPays",
    "Gateways": {
      "VakifPays": { "Enabled": true, "IsDefault": true, "IsTestMode": true, "Settings": {} },
      "Iyzico": { "Enabled": true, "IsTestMode": true, "Settings": {} },
      "Vakifbank": { "Enabled": true, "IsTestMode": true, "Settings": {} }
    }
  }
}
```


| Alan             | Zorunlu           | Açıklama                                 |
| ---------------- | ----------------- | ---------------------------------------- |
| `DefaultGateway` | ✔️                | `PaymentGatewayNames.*`                  |
| `Gateways`       | ✔️                | Kanal kodu → ayar bloğu                  |
| `Enabled`        | ✔️                | `false` ise provider devre dışı          |
| `IsTestMode`     | ✔️                | Sandbox URL                              |
| `Settings`       | ✔️                | Kanala özel key-value (§7.7)             |
| `Redis`          | ✔️ (Vakıfbank 3D) | §7.8 — 3D sonrası satış durumu önbelleği |


```csharp
request.GatewayName = PaymentGatewayNames.Iyzico; // config değil — kanal seçimi
```

---

### 7.6. Redis önbellek (Vakıfbank 3D satış durumu)

Vakıfbank akışında MPI enrollment ile VPOS satışı arasında **CVV, tutar ve son kullanma** gibi alanlar geçici olarak saklanır. TriPay bunu `**IMemoryCache` değil, Redis** (`IDistributedCache` + StackExchange.Redis) ile yapar; böylece yük dengeleme altında birden fazla API örneği aynı 3D oturumunu okuyabilir.


| Ayar (`TriPay:Redis`) | Zorunlu | Açıklama                                                                             |
| --------------------- | ------- | ------------------------------------------------------------------------------------ |
| `Configuration`       | ✔️*     | Redis bağlantı dizesi (ör. `localhost:6379`). *Alternatif: `ConnectionStrings:Redis` |
| `InstanceName`        | Hayır   | Anahtar öneki; varsayılan `tripay:`                                                  |
| `SaleStateTtlHours`   | Hayır   | Satış durumu TTL (saat); varsayılan `24`                                             |


Redis anahtar formatı: `{InstanceName}vakifbank:sale:{orderCode}` (ör. `tripay:vakifbank:sale:ORDER-123`).

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "TriPay": {
    "Redis": {
      "Configuration": "localhost:6379",
      "InstanceName": "tripay:",
      "SaleStateTtlHours": 24
    },
    "Gateways": {
      "Vakifbank": {
        "Enabled": true,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "...",
          "MerchantPassword": "...",
          "TerminalNo": "..."
        }
      }
    }
  }
}
```

Vakıfbank 3D state için Redis, **`AddTriPayFramework`** veya **`AddTriPayHosted`** (içeride `AddTriPayRedis`) ile kayıt olur. Yalnız `AddTriPay()` Redis **eklemez**.

**Yerel geliştirme:** `docker run -d --name tripay-redis -p 6379:6379 redis:7-alpine`

---

### 7.7. Config şablonları (A–F)

#### Şablon A — API Key + Secret

```json
"Iyzico": {
  "Enabled": true,
  "IsTestMode": true,
  "Settings": {
    "ApiKey": "sandbox-api-key",
    "SecretKey": "sandbox-secret-key"
  }
}
```

#### Şablon B — Nestpay / EST (çoğu banka)

```json
"IsBankasi": {
  "Enabled": true,
  "IsTestMode": true,
  "Settings": {
    "MerchantId": "000000000000001",
    "TerminalId": "00000001",
    "Username": "api_user",
    "Password": "***",
    "StoreKey": "3d_store_key_hex"
  }
}
```

#### Şablon C — Garanti BBVA

```json
"Garanti": {
  "Enabled": true,
  "IsTestMode": true,
  "Settings": {
    "MerchantId": "000000000000001",
    "TerminalId": "00000001",
    "ProvUserId": "PROVAUT",
    "ProvPassword": "***",
    "StoreKey": "3d_store_key"
  }
}
```

#### Şablon D — Vakıfbank (MPI + VPOS XML)

```json
"Vakifbank": {
  "Enabled": true,
  "IsTestMode": true,
  "Settings": {
    "MerchantId": "000000000000001",
    "MerchantPassword": "***",
    "TerminalNo": "VP000001",
    "EnrollmentUrl": "https://3dsecuretest.vakifbank.com.tr/MPIAPI/MPI_Enrollment.aspx",
    "VerifyUrl": "https://onlineodemetest.vakifbank.com.tr/VposService/v3/Vposreq.aspx",
    "InstallmentCounts": "3,6,9",
    "BinPrefixes": "454360,411979"
  }
}
```

#### Şablon E — VakıfPayS

```json
"VakifPays": {
  "Enabled": true,
  "IsDefault": true,
  "IsTestMode": true,
  "Settings": {
    "Merchant": "10009011",
    "MerchantUser": "api@firma.com",
    "MerchantPassword": "***"
  }
}
```

#### Şablon F — PayTR

```json
"PayTR": {
  "Enabled": true,
  "IsTestMode": true,
  "Settings": {
    "MerchantId": "000000",
    "MerchantKey": "***",
    "MerchantSalt": "***"
  }
}
```

---

### 7.8. Olması gerekenler — Kullanılabilir Sanal POS config örnekleri (§6)

Tam liste: [§6 proje dokümanı](./TriPay_Proje_Dokumani.md#6-olması-gerekenler--kullanılabilir-sanal-poslar) · MVP TODO: [pwd.md](../pwd.md)

#### MVP (§6.1)


| Sanal POS | `PaymentGatewayNames` | Şablon | Durum      |
| --------- | --------------------- | ------ | ---------- |
| Iyzico    | `Iyzico`              | A      | TODO P1    |
| Vakıfbank | `Vakifbank`           | D      | TODO P2    |
| VakıfPayS | `VakifPays`           | E      | **Mevcut** |


Iyzico test: `https://sandbox-api.iyzipay.com` · Prod: `https://api.iyzipay.com`

#### Bankalar (planlanan)


| Sanal POS            | `PaymentGatewayNames` | Şablon |
| -------------------- | --------------------- | ------ |
| Akbank               | `Akbank`              | B      |
| Akbank Nestpay       | `AkbankNestpay`       | B      |
| Alternatif Bank      | `AlternatifBank`      | B      |
| Anadolubank          | `Anadolubank`         | B      |
| Denizbank            | `Denizbank`           | B      |
| QNB Finansbank       | `QNBFinansbank`       | B      |
| Finansbank Nestpay   | `FinansbankNestpay`   | B      |
| Garanti BBVA         | `Garanti`             | C      |
| Halkbank             | `Halkbank`            | B      |
| ING Bank             | `ING`                 | B      |
| İş Bankası           | `IsBankasi`           | B      |
| Şekerbank            | `Sekerbank`           | B      |
| Türk Ekonomi Bankası | `TurkEkonomiBankasi`  | B      |
| Türkiye Finans       | `TurkiyeFinans`       | B      |
| Yapı Kredi Bankası   | `YapiKredi`           | B      |
| Ziraat Bankası       | `Ziraat`              | B      |
| Kuveyt Türk          | `KuveytTurk`          | B      |
| Vakıf Katılım        | `VakifKatilim`        | B      |


Nestpay bankalarında §7.7 **Şablon B** kullanılır; yalnızca `Gateways` anahtarı (`"Halkbank"`, `"Ziraat"`, …) değişir.

#### Ödeme kuruluşları (planlanan)


| Sanal POS    | `PaymentGatewayNames` | Şablon |
| ------------ | --------------------- | ------ |
| Cardplus     | `Cardplus`            | A      |
| Paratika     | `Paratika`            | A      |
| Payten - MSU | `PaytenMsu`           | A      |
| Sipay        | `Sipay`               | A      |
| QNBpay       | `QNBpay`              | A      |
| ParamPos     | `ParamPos`            | A      |
| PayBull      | `PayBull`             | A      |
| Parolapara   | `Parolapara`          | A      |
| IQmoney      | `IQmoney`             | A      |
| Ahlpay       | `Ahlpay`              | A      |
| Moka         | `Moka`                | A*     |
| Vepara       | `Vepara`              | A      |
| ZiraatPay    | `ZiraatPay`           | A      |
| Tami         | `Tami`                | A      |
| HalkÖde      | `HalkOde`             | A      |
| PayNKolay    | `PayNKolay`           | A      |
| Paynet       | `Paynet`              | A      |
| PayTR        | `PayTR`               | F      |


 Moka: kuruluş dokümanına göre `DealerCode`, `Username`, `Password` eklenebilir.

**Sipay tipi örnek:**

```json
"Sipay": {
  "Enabled": false,
  "IsTestMode": true,
  "Settings": {
    "MerchantId": "SP_MERCHANT",
    "ApiKey": "sipay-api-key",
    "SecretKey": "sipay-secret"
  }
}
```

#### Birden fazla kanal — tam `appsettings` örneği

```json
{
  "TriPay": {
    "DefaultGateway": "VakifPays",
    "Gateways": {
      "VakifPays": {
        "Enabled": true,
        "IsDefault": true,
        "IsTestMode": true,
        "Settings": {
          "Merchant": "10009011",
          "MerchantUser": "api@firma.com",
          "MerchantPassword": "***"
        }
      },
      "Iyzico": {
        "Enabled": true,
        "IsTestMode": true,
        "Settings": {
          "ApiKey": "sandbox-key",
          "SecretKey": "sandbox-secret"
        }
      },
      "Vakifbank": {
        "Enabled": true,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "000000000000001",
          "MerchantPassword": "***",
          "TerminalNo": "VP000001"
        }
      },
      "Garanti": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "GAR_ID",
          "TerminalId": "GAR_TERM",
          "ProvUserId": "PROVAUT",
          "ProvPassword": "***",
          "StoreKey": "store_key"
        }
      },
      "IsBankasi": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "ISB_ID",
          "TerminalId": "ISB_TERM",
          "Username": "user",
          "Password": "***",
          "StoreKey": "store_key"
        }
      },
      "PayTR": {
        "Enabled": false,
        "IsTestMode": true,
        "Settings": {
          "MerchantId": "123456",
          "MerchantKey": "***",
          "MerchantSalt": "***"
        }
      }
    }
  }
}
```

```mermaid
flowchart LR
    A[appsettings / DB / Vault] --> B[TriPay Gateways]
    B --> C[PaymentGatewayFactory]
    C --> D[Provider adaptörleri]
    E[İstek GatewayName] --> C
```



> Trimango: her provider `Settings` sözlüğünü `GetGatewayConfigAsync()` ile okur. TriPay hedefi: `TriPay:Gateways:{code}:Settings` + `MerchantGateways`.

---

## 8. Provider (banka) seçimi

Developer hangi banka/kuruluşun kullanılacağını **üç şekilde** belirler (detay: proje dokümanı §5.5).

### 8.1. `PaymentGatewayNames` sabitleri (zorunlu)

Gateway kodları **magic string olamaz**. `TriPay.Services.PaymentGatewayNames` sınıfındaki `const` değerler kullanılır:

```csharp
using TriPay.Services;

PaymentGatewayNames.VakifPays   // mevcut
PaymentGatewayNames.Iyzico      // planlanan
PaymentGatewayNames.Garanti     // planlanan
PaymentGatewayNames.Default     // varsayılan (= VakifPays)
```


| Yöntem            | Örnek                                                           |
| ----------------- | --------------------------------------------------------------- |
| İstekte açık ad   | `GatewayName = PaymentGatewayNames.VakifPays`                   |
| Varsayılan kanal  | `PaymentGatewayNames.Default` veya `MerchantGateways.IsDefault` |
| Metot parametresi | `InitializePaymentAsync(request, PaymentGatewayNames.Iyzico)`   |


```csharp
using TriPay.Services;

var request = new PaymentGatewayInitializeRequestDto
{
    GatewayName = PaymentGatewayNames.VakifPays,
    Payment = paymentModel
};

var result = await _payment.InitializePaymentAsync(request);
// veya
var result = await _payment.InitializePaymentAsync(request, PaymentGatewayNames.VakifPays);
```

**Kurallar:**

- `"VakifPays"` gibi string literal **yasak** — yalnızca `PaymentGatewayNames.`*
- Yeni tip eklerken **bir dosya = bir `public` tip** (Kural #14)
- `GatewayName` factory’de kayıtlı olmalı (`GetAllAvailableGateways()`)
- Yeni kanal: `PaymentGatewayNames` + Factory + provider birlikte güncellenir
- İleride: merchant için `IsEnabled` kontrolü `IPaymentGatewaySelector` ile yapılacak

---

## 9. Temel kavramlar ve modeller

### 9.1. `PaymentRequest` — ödeme bilgisi


| Alan                                             | Zorunlu | Açıklama                      |
| ------------------------------------------------ | ------- | ----------------------------- |
| `OrderNumber`                                    | ✔️      | Benzersiz sipariş no          |
| `Amount`                                         | ✔️      | Tutar                         |
| `Currency`                                       |         | `TRY` (varsayılan)            |
| `CardNumber`, `ExpiryMonth`, `ExpiryYear`, `Cvv` | ✔️*     | *3D/sale için                 |
| `CardOwner`                                      | ✔️      | Kart sahibi adı               |
| `CustomerEmail`, `CustomerPhone`, `CustomerIp`   | ✔️      | Müşteri bilgisi               |
| `ReturnUrl`                                      | ✔️ (3D) | Callback URL — sizin endpoint |
| `InstallmentCount`                               |         | Taksit (varsayılan 1)         |
| `Use3D`                                          |         | `true` → 3D Secure            |
| `TestPlatform`                                   |         | Test ortamı bayrağı           |


### 9.2. `Result<T>` — sonuç

```csharp
if (!result.IsSuccess)
{
    // result.ErrorMessage
    return;
}
var data = result.Data;
```

### 9.3. Gateway adları (kodda kayıtlı / hedef)


| GatewayName                     | Durum          |
| ------------------------------- | -------------- |
| `VakifPays`                     | Mevcut         |
| `Iyzico`, `Garanti`, `PayTR`, … | Planlanan (§6) |


---

## 10. Ödeme başlatma (Initialize)

```csharp
using TriPay.Services;
using TriPay.Services.Interfaces;
using TriPay.Services.Models;
using TriPay.Services.Providers;

var payment = new PaymentRequest
{
    OrderNumber = "ORD-" + Guid.NewGuid().ToString("N")[..12],
    Amount = 1500.00m,
    Currency = "TRY",
    InstallmentCount = 1,
    CardOwner = "Mehmet Unal",
    CardNumber = "4111111111111111",
    ExpiryMonth = "12",
    ExpiryYear = "2028",
    Cvv = "123",
    CustomerName = "Mehmet Unal",
    CustomerEmail = "musteri@ornek.com",
    CustomerPhone = "5555555555",
    CustomerIp = "127.0.0.1",
    ReturnUrl = "https://magaza.com/payment/callback",
    TestPlatform = true,
    Use3D = true
};

var request = new PaymentGatewayInitializeRequestDto
{
    GatewayName = PaymentGatewayNames.VakifPays,
    Payment = payment
};

var result = await _paymentGatewayService.InitializePaymentAsync(request);

if (!result.IsSuccess)
    throw new Exception(result.ErrorMessage);

var init = result.Data!;
if (!string.IsNullOrEmpty(init.RedirectHtml))
{
    // 3D: HTML auto-post — tarayıcıya yaz veya Content(init.RedirectHtml, "text/html")
}
else if (!string.IsNullOrEmpty(init.RedirectUrl))
{
    // Redirect(init.RedirectUrl)
}
else
{
    // Non-3D doğrudan sonuç: init.Success, init.Message
}
```

---

## 11. 3D Secure akışı

```mermaid
sequenceDiagram
    participant U as Kullanıcı
    participant App as Sizin Uygulama
    participant TP as IPaymentGatewayService
    participant Bank as Banka 3D

    U->>App: Ödeme formu
    App->>TP: InitializePaymentAsync
    TP-->>App: RedirectHtml
    App-->>U: Auto-post sayfa
    U->>Bank: SMS / onay
    Bank->>App: POST ReturnUrl (Callback)
    App->>TP: ProcessCallbackAsync
    App->>TP: GetPaymentStatusAsync
```



**ReturnUrl:** TriPay değil, **sizin** controller adresiniz olmalı (`/payment/callback`).

---

## 12. Callback işleme

Banka `ReturnUrl`’e `application/x-www-form-urlencoded` POST eder.

```csharp
[HttpPost]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
public async Task<IActionResult> Callback(IFormCollection form)
{
    var raw = form.Keys.ToDictionary(k => k, k => form[k].ToString() ?? string.Empty);

    var callbackResult = await _payment.ProcessCallbackAsync(
        new PaymentGatewayCallbackRequestDto
        {
            GatewayName = PaymentGatewayNames.VakifPays,
            RawData = raw
        });

    if (!callbackResult.IsSuccess)
        return BadRequest(callbackResult.ErrorMessage);

    var cb = callbackResult.Data!;

    // Ek doğrulama: banka sorgusu
    var statusResult = await _payment.GetPaymentStatusAsync(cb.OrderNumber, PaymentGatewayNames.VakifPays);
    var status = statusResult.Data;

    var success = cb.Success && status?.Success == true && status.ResponseCode == "00";

    return View("Result", new { success, cb.OrderNumber, cb.TransactionId });
}
```

**Normalize (ham veri ayrıştırma):**

```csharp
var normalized = await _payment.NormalizeCallbackFromRawDataAsync(PaymentGatewayNames.VakifPays, raw);
// Status, PaymentId, ErrorCode, ErrorMessage, ...
```

---

## 13. Taksit sorgulama

```csharp
var result = await _payment.GetInstallmentInfoAsync(
    new PaymentGatewayInstallmentRequestDto
    {
        GatewayName = PaymentGatewayNames.VakifPays,
        CardNumber = "4111111111111111",
        Amount = 6000m,
        Currency = "TRY",
        TestPlatform = true
    });

if (result.IsSuccess && result.Data != null)
{
    foreach (var opt in result.Data.Installments)
    {
        // opt.Count, opt.Monthly, opt.Total, opt.Label
    }
}
```

**ASP.NET Core JSON endpoint:**

```csharp
[HttpGet]
public async Task<IActionResult> Installments(string cardNumber, decimal amount = 0)
{
    var result = await _payment.GetInstallmentInfoAsync(new PaymentGatewayInstallmentRequestDto
    {
        CardNumber = cardNumber,
        Amount = amount,
        GatewayName = PaymentGatewayNames.VakifPays,
        TestPlatform = true
    });

    return Json(new
    {
        success = result.IsSuccess,
        installments = result.Data?.Installments ?? []
    });
}
```

---

## 14. Ödeme durumu sorgulama

```csharp
var result = await _payment.GetPaymentStatusAsync(
    paymentId: "TRIPAY-2024-001",   // veya banka transaction id
    gatewayName: PaymentGatewayNames.VakifPays);

if (result.IsSuccess)
{
    var q = result.Data!;
    // q.Success, q.ResponseCode, q.Message
}
```

Callback sonrası **çift doğrulama** için kullanılır.

---

## 15. İade (Refund)

```csharp
// Tam iade
var result = await _payment.RefundPaymentAsync(
    paymentId: "pgTranId-123456",
    gatewayName: PaymentGatewayNames.VakifPays);

// Kısmi iade
var partial = await _payment.RefundPaymentAsync(
    paymentId: "pgTranId-123456",
    amount: 50.00m,
    gatewayName: PaymentGatewayNames.VakifPays);
```

---

## 16. Aktif gateway listesi

```csharp
// Factory’de kayıtlı tüm adlar
IReadOnlyList<string> all = _factory.GetAllAvailableGateways();

// Sistemde aktif provider adları
IReadOnlyList<string> systemActive = _payment.GetSystemActiveGatewayNames();

// Desteklenen ve çalışır durumda olanlar (async)
IReadOnlyList<string> active = await _payment.GetActiveGatewaysAsync();
```

Checkout’ta kullanıcıya gösterilecek banka listesi için `GetActiveGatewaysAsync` + merchant `IsEnabled` filtresi (planlanan) kullanılır.

---

## 17. HttpClient ile kullanım (uzak API)

Kütüphaneyi referans almadan, **uzaktaki TriPay REST API**’sine `HttpClient` ile bağlanma (Model B).

### 17.1. Ne zaman?

- TriPay ayrı sunucuda host ediliyorsa,
- PHP / Node / Java gibi .NET dışı projeler (TriPay API üzerinden),
- Merkezi ödeme mikroservisi.

### 17.2. HttpClient kaydı (.NET consumer)

```csharp
builder.Services.AddHttpClient<ITriPayApiClient, TriPayApiClient>(client =>
{
    client.BaseAddress = new Uri("https://payment-api.firma.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(60);
});
```

### 17.3. Kimlik doğrulama (önerilen)


| Header                 | Açıklama                             |
| ---------------------- | ------------------------------------ |
| `Authorization`        | `Bearer {merchantApiKey}`            |
| `X-TriPay-Merchant-Id` | Üye işyeri id                        |
| `X-TriPay-Gateway`     | Opsiyonel; varsayılan kanal override |


### 17.4. REST uçları (hedef sözleşme)

> TriPay REST API henüz ayrı host olarak yayınlanmamış olabilir; sözleşme entegrasyon için hedeftir. In-process kullanım için [§10–15](#10-ödeme-başlatma-initialize) yeterlidir.


| Metot  | Endpoint                                | Gövde                                     |
| ------ | --------------------------------------- | ----------------------------------------- |
| `POST` | `/api/v1/payments/initialize`           | `PaymentGatewayInitializeRequestDto` JSON |
| `POST` | `/api/v1/payments/callback`             | Form veya JSON `RawData`                  |
| `GET`  | `/api/v1/payments/{orderNumber}/status` | —                                         |
| `GET`  | `/api/v1/installments`                  | `?cardNumber=&amount=&gateway=`           |
| `POST` | `/api/v1/payments/{id}/refund`          | `{ "amount": null }`                      |
| `GET`  | `/api/v1/merchants/{id}/gateways`       | Aktif kanal listesi                       |


### 17.5. Örnek: Initialize (HttpClient)

```csharp
public class TriPayApiClient : ITriPayApiClient
{
    private readonly HttpClient _http;

    public TriPayApiClient(HttpClient http) => _http = http;

    public async Task<PaymentGatewayInitializeResponseDto> InitializeAsync(
        PaymentGatewayInitializeRequestDto request,
        CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/v1/payments/initialize", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaymentGatewayInitializeResponseDto>(ct)
               ?? throw new InvalidOperationException("Boş cevap");
    }
}
```

### 17.6. Örnek: Callback (form POST proxy)

Banka sizin sitenize değil TriPay API’ye post edecekse URL TriPay host’ta olur; siz yalnızca webhook ile sonuç alırsınız. Kendi sitenizde callback alıyorsanız **Model A (DLL)** daha uygundur.

### 17.7. Model A vs B karar tablosu


| Kriter      | NuGet/DLL                              | HttpClient API                 |
| ----------- | -------------------------------------- | ------------------------------ |
| Latency     | Düşük                                  | Ağ hop +1                      |
| Bağımlılık  | .NET + TriPay DLL                      | Yalnızca HTTP                  |
| Kart verisi | Sizin sunucunuzdan bankaya (PCI sizde) | TriPay host’ta (PCI TriPay’de) |
| Güncelleme  | NuGet sürüm bump                       | API versiyon                   |


---

## 18. ASP.NET Core MVC entegrasyonu

Minimum dosyalar:


| Dosya                          | Görev                             |
| ------------------------------ | --------------------------------- |
| `Program.cs`                   | `AddTriPayHosted()` — [örnek](./TriPay_Program_cs_ve_DI.md#5-aspnet-core-web--hosted-demo--operatör) |
| `CheckoutController.cs`        | `Pay`, `Callback`, `Installments` |
| `Views/Checkout/Index.cshtml`  | Ödeme formu                       |
| `Views/Checkout/Result.cshtml` | Sonuç                             |


**Pay action özeti:**

```csharp
[HttpPost]
public async Task<IActionResult> Pay(PaymentRequest model)
{
    var result = await _payment.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
    {
        GatewayName = model.GatewayName ?? PaymentGatewayNames.Default,
        Payment = model
    });

    if (!result.IsSuccess)
    {
        ModelState.AddModelError("", result.ErrorMessage ?? "Hata");
        return View("Index", model);
    }

    if (!string.IsNullOrWhiteSpace(result.Data?.RedirectHtml))
        return Content(result.Data.RedirectHtml, "text/html; charset=utf-8");

    return View("Result", result.Data);
}
```

**Referans implementasyon (TriPay demo):** `TriPay/Controllers/CheckoutController.cs` (`Pay`, `Callback`, `Installments`). `HomeController` yalnızca ana sayfa yönlendirmesi ve `Privacy` içindir.

---

## 19. Console / Worker Service örneği

Console ve Worker’da da **`AddTriPayFramework(configuration)`** kullanın. Eski örnekteki yalnız `AddTriPay()` eksik kayıt bırakır.

Tam kod: [TriPay_Program_cs_ve_DI.md §6](./TriPay_Program_cs_ve_DI.md#6-console--worker--framework)

---

## 20. Hata yönetimi


| Durum               | Davranış                                                 |
| ------------------- | -------------------------------------------------------- |
| Provider bulunamadı | `Result.Failure("Payment gateway provider bulunamadı.")` |
| Banka hata kodu     | `Data.Success == false`, `Message` / `ErrorMessage`      |
| Exception           | try/catch — loglama `TransactionLogs` (planlanan)        |


**Önerilen pattern:**

```csharp
var result = await _payment.InitializePaymentAsync(request);
if (!result.IsSuccess)
{
    _logger.LogWarning("Ödeme başlatılamadı: {Error}", result.ErrorMessage);
    return BadRequest(new { error = result.ErrorMessage });
}
```

Planlanan hata kodları (§5.5): `GATEWAY_NOT_ENABLED_FOR_MERCHANT`, `GATEWAY_NOT_REGISTERED`, vb.

---

## 21. Güvenlik ve PCI


| Kural              | Uygulama                                                               |
| ------------------ | ---------------------------------------------------------------------- |
| Kart verisi        | Loglara ve MSSQL’e **yazılmaz** — `PciDataMasker.MaskSensitivePayload` |
| HTTPS              | Zorunlu (ReturnUrl, API)                                               |
| Callback           | `[IgnoreAntiforgeryToken]` — banka POST’u                              |
| Callback replay    | `RedisIdempotencyStore` — `PaymentGatewayService.ProcessCallbackAsync` |
| API key            | User Secrets / K8s Secret / Key Vault                                  |
| Üye işyeri webhook | `WebhookSignatureHelper` — HMAC-SHA256                                 |
| DLL güncelleme     | Güvenlik yamaları için NuGet sürüm takibi                              |


**Tam mimari (işlem state machine, RabbitMQ outbox, Docker, Kubernetes):**  
[TriPay_Guvenlik_ve_Altrapi_Dokumani.md](./TriPay_Guvenlik_ve_Altrapi_Dokumani.md)

### 21.1. Yerel altyapı

```bash
docker compose up -d   # redis:6379, rabbitmq:5672/15672, mssql:1433
```

`TriPay:RabbitMq` ve `TriPay:Redis:IdempotencyTtlDays` — `appsettings.json` örneği.

---

## 22. Test ortamı


| Ayar                                  | Değer                                                            |
| ------------------------------------- | ---------------------------------------------------------------- |
| `PaymentRequest.TestPlatform`         | `true`                                                           |
| `InstallmentInfoRequest.TestPlatform` | `true`                                                           |
| Gateway                               | `PaymentGatewayNames.VakifPays` (test credential — provider içi) |


Test kartları: ilgili banka / VakıfPayS test dokümantasyonu.

---

## 23. Sık sorulan sorular

**NuGet mi DLL mi?**  
Üretimde NuGet; lokal/debug’da ProjectReference veya DLL.

**HttpClient mi DLL mi?**  
Aynı .NET uygulamasında bankaya doğrudan gidiyorsanız DLL. TriPay ayrı sunucudaysa HttpClient.

**Birden fazla banka?**  
`GatewayName` veya merchant panelinden `IsEnabled` kanallar (§5.5 proje dokümanı).

**Callback URL kimde?**  
Sizin domain’inizde bir endpoint; `ReturnUrl` olarak verilir.

**TriPay.Web zorunlu mu?**  
Hayır; kütüphane doğrudan projenize eklenir.

---

## 24. Hızlı referans tablosu


| İşlem        | Servis metodu                       | Gateway parametresi                     |
| ------------ | ----------------------------------- | --------------------------------------- |
| Ödeme başlat | `InitializePaymentAsync`            | `request.GatewayName` veya 2. parametre |
| Callback     | `ProcessCallbackAsync`              | `request.GatewayName`                   |
| Taksit       | `GetInstallmentInfoAsync`           | `request.GatewayName`                   |
| Durum        | `GetPaymentStatusAsync`             | 2. parametre `gatewayName`              |
| İade         | `RefundPaymentAsync`                | 3. parametre `gatewayName`              |
| Normalize    | `NormalizeCallbackFromRawDataAsync` | 1. parametre `gatewayName`              |
| Liste        | `GetActiveGatewaysAsync`            | —                                       |


**DI (üretim):** `AddTriPayFramework` veya `AddTriPayHosted` — [TriPay_Program_cs_ve_DI.md](./TriPay_Program_cs_ve_DI.md)  
**Ana arayüz:** Framework → `IPaymentGatewayService` · Hosted → `IPaymentCheckoutService`  
**DLL:** `TriPay.Services.dll`  
**NuGet (hedef):** `TriPay`

---

**Hazırlayan:** TriPay Geliştirme Ekibi  
**Tarih:** 22 Mayıs 2026  
**Mimari detay:** [TriPay_Proje_Dokumani.md](./TriPay_Proje_Dokumani.md)
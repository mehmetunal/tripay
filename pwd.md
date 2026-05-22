# TriPay — Proje Çalışma Dokümanı (pwd)

> Güncel mimari ve kurallar: [docs/TriPay_Proje_Dokumani.md](./docs/TriPay_Proje_Dokumani.md) · §6 Kullanılabilir Sanal POS  
> Entegrasyon: [docs/TriPay_Kullanim_Kilavuzu.md](./docs/TriPay_Kullanim_Kilavuzu.md)

---

## Öncelik — MVP (§6.1)

Sıra **bağlayıcıdır**. Yeni adaptörler Trimango `PaymentGateways/Providers` dosyalarından TriPay `TriPay.Services/Providers` yapısına port edilir.

### MVP — yapılacak sıra (P1–P3)

| Öncelik | Kanal | `PaymentGatewayNames` | Durum |
| :---: | :--- | :--- | :---: |
| **1** | **iyzico** | `Iyzico` | ⬜ Yapılacak |
| **2** | **Vakıfbank** | `Vakifbank` | ⬜ Yapılacak |
| **3** | **VakıfPayS** | `VakifPays` | ✅ Tamamlandı |

#### Trimango kaynak (MVP port)

| Kanal | Dosya |
| :--- | :--- |
| iyzico | `/Users/mehmet/Project/trimango/src/Libraries/Trimango.Services/PaymentGateways/Providers/IyzicoGatewayProvider.cs` |
| Vakıfbank | `/Users/mehmet/Project/trimango/src/Libraries/Trimango.Services/PaymentGateways/Providers/VakifbankGatewayProvider.cs` |
| VakıfPayS | `TriPay.Services/Providers/VakifPaysGatewayProvider.cs` (mevcut) |

---

### §6 — Diğer kullanılabilir Sanal POS'lar (backlog)

> Kaynak: [TriPay_Proje_Dokumani.md §6](./docs/TriPay_Proje_Dokumani.md#6-olması-gerekenler--kullanılabilir-sanal-poslar)  
> **Semboller:** ✔️ hedef · ❌ ilk fazda yok · Durum: `Planlanan` (MVP tamamlandıktan sonra)

**TriPay kod durumu:** `Planlanan` = adaptör yok · `TODO P1/P2` / `Mevcut` = yalnızca MVP satırlarında

#### Bankalar

| Sanal POS | `PaymentGatewayNames` | Satış | 3D | İptal | İade | Durum |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| Akbank | `Akbank` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Akbank Nestpay | `AkbankNestpay` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Alternatif Bank | `AlternatifBank` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Anadolubank | `Anadolubank` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Denizbank | `Denizbank` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| QNB Finansbank | `QNBFinansbank` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Finansbank Nestpay | `FinansbankNestpay` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Garanti BBVA | `Garanti` | ✔️ | ✔️ | ❌ | ❌ | ⬜ Planlanan |
| Halkbank | `Halkbank` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| ING Bank | `ING` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| İş Bankası | `IsBankasi` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Şekerbank | `Sekerbank` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Türk Ekonomi Bankası | `TurkEkonomiBankasi` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Türkiye Finans | `TurkiyeFinans` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Yapı Kredi Bankası | `YapiKredi` | ✔️ | ✔️ | ❌ | ❌ | ⬜ Planlanan |
| Ziraat Bankası | `Ziraat` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Kuveyt Türk | `KuveytTurk` | ✔️ | ✔️ | ❌ | ❌ | ⬜ Planlanan |
| Vakıf Katılım | `VakifKatilim` | ✔️ | ✔️ | ❌ | ❌ | ⬜ Planlanan |

#### Ödeme kuruluşları

| Sanal POS | `PaymentGatewayNames` | Satış | 3D | İptal | İade | Durum |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| Cardplus | `Cardplus` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Paratika | `Paratika` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Payten - MSU | `PaytenMsu` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Sipay | `Sipay` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| QNBpay | `QNBpay` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| ParamPos | `ParamPos` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| PayBull | `PayBull` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Parolapara | `Parolapara` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| IQmoney | `IQmoney` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Ahlpay | `Ahlpay` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Moka | `Moka` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Vepara | `Vepara` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| ZiraatPay | `ZiraatPay` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Tami | `Tami` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| HalkÖde | `HalkOde` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| PayNKolay | `PayNKolay` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |
| Paynet | `Paynet` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ Planlanan |

#### MVP'de listelenen (üst tabloda takip)

| Sanal POS | `PaymentGatewayNames` | Satış | 3D | İptal | İade | Durum |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| Iyzico | `Iyzico` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ **TODO P1** |
| Vakıfbank | `Vakifbank` | ✔️ | ✔️ | ✔️ | ✔️ | ⬜ **TODO P2** |
| VakıfPayS | `VakifPays` | ✔️ | ✔️ | ✔️ | ✔️ | ✅ **Mevcut** |

**Notlar (§6 ile uyumlu):**

- Garanti BBVA, Yapı Kredi, Kuveyt Türk, Vakıf Katılım: iptal/iade ❌ — sonraki faz veya banka API kısıtı.
- `PaymentGatewayNames` içinde tanımlı, §6 tablosunda ayrı satır yok: `PayTR` — backlog’a eklenecekse doküman §6 ile senkron güncellenmeli.
- Her backlog kanalı için: `{Kanal}GatewayProvider.cs` + Factory + `AddTriPay()` + §6 tablo `Mevcut`.

#### Backlog — provider checkbox (özet)

**Bankalar**

- [ ] Akbank · [ ] Akbank Nestpay · [ ] Alternatif Bank · [ ] Anadolubank · [ ] Denizbank
- [ ] QNB Finansbank · [ ] Finansbank Nestpay · [ ] Garanti BBVA · [ ] Halkbank · [ ] ING Bank
- [ ] İş Bankası · [ ] Şekerbank · [ ] Türk Ekonomi Bankası · [ ] Türkiye Finans
- [ ] Yapı Kredi · [ ] Ziraat · [ ] Kuveyt Türk · [ ] Vakıf Katılım

**Ödeme kuruluşları**

- [ ] Cardplus · [ ] Paratika · [ ] Payten MSU · [ ] Sipay · [ ] QNBpay · [ ] ParamPos
- [ ] PayBull · [ ] Parolapara · [ ] IQmoney · [ ] Ahlpay · [ ] Moka · [ ] Vepara
- [ ] ZiraatPay · [ ] Tami · [ ] HalkÖde · [ ] PayNKolay · [ ] Paynet

---

## TODO listesi

### P1 — iyzico

- [ ] `TriPay.Services/Providers/IyzicoGatewayProvider.cs` — Trimango `IyzicoGatewayProvider` port
- [ ] Gerekirse `IyzicoService.cs` (HTTP, IYZWSv2 imza, `sandbox-api.iyzipay.com` / `api.iyzipay.com`)
- [ ] `PaymentGatewayFactory`: `[PaymentGatewayNames.Iyzico] = typeof(IyzicoGatewayProvider)`
- [ ] `AddTriPay()`: `IyzicoGatewayProvider` + `AddHttpClient`
- [ ] Ayarlar: `ApiKey`, `SecretKey`, `IsTestMode` (`IOptions<TriPayGatewayOptions>` veya config bölümü `TriPay:Gateways:Iyzico`)
- [ ] Metotlar: `InitializePayment`, `ProcessCallback`, `GetInstallmentInfo`, `Auth3DS`, `GetPaymentStatus`, `RefundPayment` (Trimango’da iade NotImplemented — TriPay’de planla)
- [ ] Trimango bağımlılıkları kaldır: `IPaymentSettingsService` → TriPay config; `Maggsoft.Core.Base.Result` → `TriPay.Services.Common.Result`; DTO’lar → `TriPay.Services.Models`
- [ ] §6 tablo: **Iyzico** → `Mevcut`

### P2 — Vakıfbank

- [ ] `TriPay.Services/Providers/VakifbankGatewayProvider.cs` — Trimango `VakifbankGatewayProvider` port
- [ ] MPI Enrollment + Vpos Verify (XML), 3D auto-submit form
- [ ] `PaymentGatewayFactory`: `[PaymentGatewayNames.Vakifbank] = typeof(VakifbankGatewayProvider)`
- [ ] `AddTriPay()`: provider kaydı
- [ ] Cache: 3D sonrası `VakifbankSaleState` (Trimango `ICache` → `IMemoryCache` veya in-memory dict MVP)
- [ ] BIN / taksit: Trimango `IPaymentGatewayBinPrefixService` → TriPay sadeleştirilmiş servis veya config listesi
- [ ] Ayarlar: `MerchantId`, `MerchantPassword`, `TerminalNo`, `EnrollmentUrl`, `VerifyUrl`, `InstallmentCounts`
- [ ] Metotlar: `InitializePayment`, `ProcessCallback`, `Auth3DS`, `GetInstallmentInfo`, `RefundPayment`, `NormalizeCallbackFromRawData`
- [ ] §6 tablo: **Vakıfbank** → `Mevcut`

### P3 — VakıfPayS (tamamlandı)

- [x] `VakifPaysGatewayProvider` + `VakifPaysService`
- [x] `PaymentGatewayFactory` + `AddTriPay()` DI
- [x] `PaymentGatewayNames.VakifPays` ve `Default`

---

## TriPay hedef dosya yapısı (provider sonrası)

```text
TriPay.Services/Providers/
├── IyzicoGatewayProvider.cs      ← P1
├── VakifbankGatewayProvider.cs   ← P2
├── VakifPaysGatewayProvider.cs     ← P3 ✅
├── VakifPaysService.cs             ← P3 ✅
└── (ileride) IyzicoService.cs, VakifbankHelper.cs
```

---

## Ortak tamamlama kontrolü (her yeni provider)

- [ ] `GatewayName` → `PaymentGatewayNames.*` (magic string yok)
- [ ] `PaymentGatewayBase` türetimi
- [ ] Factory dictionary kaydı
- [ ] `AddTriPay()` scoped + HttpClient
- [ ] Demo `HomeController` / config örneği (opsiyonel)
- [ ] `docs/TriPay_Proje_Dokumani.md` §6 tablo durumu güncelle
- [ ] Bu dosyadaki (pwd) checkbox işaretle

---

**Son güncelleme:** 2026-05-22

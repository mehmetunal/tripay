# TriPay — Admin Panel ve Veritabanı (Hosted Mod)

> **İlişkili:** [TriPay_Kapsam_ve_Entegrasyon_Modelleri.md](./TriPay_Kapsam_ve_Entegrasyon_Modelleri.md) · [TriPay_Proje_Dokumani.md §17](./TriPay_Proje_Dokumani.md#17-yönetim-paneli-admin--en-son-faz) · [TriPay_Kullanim_Kilavuzu.md](./TriPay_Kullanim_Kilavuzu.md)

**Versiyon:** 1.1 · **Tarih:** 22 Mayıs 2026

Bu doküman yalnızca **Hosted mod** (`AddTriPayHosted`) için geçerlidir.  
**Yönetim paneli UI:** `TriPay.Admin` — **Tailwind CSS 4** + **Gulp** (`gulpfile.js` → `admin.min.css`, `admin.min.js`). Bootstrap ve jQuery kullanılmaz. Bkz. [TriPay_Admin_Fazlar.md](./TriPay_Admin_Fazlar.md). **Framework (NuGet)** kullanan üye işyerlerinin TriPay MSSQL’i **yoktur**; veri kendi sistemlerinde tutulur.

---

## 1. Hangi veritabanı?

| Ortam | Veritabanı | Sahip |
| :--- | :--- | :--- |
| TriPay Hosted / SaaS | **Microsoft SQL Server** (`ConnectionStrings:TriPay`) | TriPay operatörü |
| Framework (NuGet) | **Yok** (TriPay tarafında) | — |
| Üye işyeri uygulaması | Kendi MSSQL/PostgreSQL vb. | Merchant (KVKK sorumlusu) |

Redis **veritabanı değildir**; önbellek ve geçici state içindir (TTL ile silinir).

---

## 2. MSSQL’de tutulan tablolar (mevcut)

### 2.1. Ödeme işlemleri

#### `Transactions` — işlem özeti

| Kolon | Kişisel veri? | Açıklama |
| :--- | :---: | :--- |
| `OrderNumber` | Dolaylı | Üye işyeri sipariş referansı |
| `Amount`, `Currency` | Hayır | Tutar |
| `Status` | Hayır | `Pending`, `Success`, `Failed` |
| `ResponseCode`, `ResponseMessage` | Hayır | Normalize sonuç |
| `ExternalTransactionId` | Hayır | Banka işlem id |
| `ClientIp` | **Evet** | Müşteri IP — KVKK’da dikkat |
| `InstallmentCount` | Hayır | Taksit |

**Tutulmaz:** Kart numarası, CVV, tam PAN.

#### `TransactionLogs` — API adım logları (opsiyonel)

`TriPay:Persistence:PersistTransactionLogs` ile açılır/kapatılır.

| Kolon | İçerik | KVKK |
| :--- | :--- | :--- |
| `RequestPayload` / `ResponsePayload` | **Maskeli** JSON/form | Düşük risk; yine de retention gerekir |
| `LogType` | `PayRequest`, `Initialize*`, `Callback*`, `Query*` | — |
| `ErrorCode`, `ErrorMessage` | Banka hata detayı | — |

**Kapatma (önerilen hafif mod):** Yalnız `Transactions` özeti; ham log yok → operasyonel debug zorlaşır, KVKK riski azalır.

#### `OutboxMessages` — webhook kuyruğu

`TriPay:Persistence:EnableOutbox` ile kontrol.

| Kolon | Açıklama |
| :--- | :--- |
| `Payload` | Merchant’a gidecek webhook JSON (kart içermez) |
| `RoutingKey` | Örn. `payment.webhook` |
| `IsPublished`, `RetryCount` | RabbitMQ dispatch durumu |

---

### 2.2. Üye işyeri ve kanallar

#### `Merchants`

| Kolon | Admin’de | Not |
| :--- | :--- | :--- |
| `Name` | Görünür / düzenlenebilir | |
| `ApiKey` | Maskeli gösterim | Entegrasyon anahtarı |
| `WebhookUrl` | Düzenlenebilir | |
| `IsActive` | Düzenlenebilir | |

#### `PaymentGateways`

Sistem kanal tanımı (VakifPays, Iyzico, Vakifbank…). Kod §6 ile uyumlu.

#### `MerchantGateways` (planlanan)

| Kolon | Açıklama |
| :--- | :--- |
| `EncryptedCredentials` | POS şifreleri — **Always Encrypted** / Vault |
| `IsEnabled`, `IsDefault`, `Priority` | Routing |

---

### 2.3. Gateway metadata (teknik, credential değil)

#### `GatewaySettings`

| Örnek `SettingKey` | Örnek değer | Ortam |
| :--- | :--- | :--- |
| `EnrollmentUrl` | MPI URL | Test / Production |
| `VerifyUrl` | VPOS URL | Test / Production |
| `ResultCodeSuccess` | `0000` | All |
| `ThreeDsStatusEnrolled` | `Y` | All |

**Admin’den düzenlenir.** Credential (`MerchantPassword`, `ApiKey`) **bu tabloda tutulmaz**.

#### `GatewayErrorMappings`

| Kolon | Açıklama |
| :--- | :--- |
| `ProviderErrorCode` | Banka kodu (ör. `0051`) |
| `UserMessage` | Son kullanıcıya Türkçe mesaj |
| `Locale` | `tr` |
| `NormalizedCode` | İç raporlama |

Redis önbellek: `gateway:settings:*`, `gateway:errors:*` — admin kaydı sonrası **invalidate** edilir.

---

### 2.4. Identity (en son faz — §17)

FluentMigrator ile eklenecek; **yalnız admin panel kullanıcıları**.

| Tablo | İçerik |
| :--- | :--- |
| `AspNetUsers` | Panel girişi |
| `AspNetRoles` | `Admin`, `Support`, `ReadOnly` |
| `AspNetUserRoles` | Atama |

Seed (Development): `admin@gmail.com` / `Super123!`

**Ödeme müşterisi** Identity tablolarında **tutulmaz**.

---

## 3. Admin panelde tutulmayan / gösterilmeyen veriler

| Veri | Nerede kalmalı? |
| :--- | :--- |
| Kart PAN, CVV | Hiçbir yerde (PCI) |
| Banka API secret, terminal şifresi | Key Vault veya `MerchantGateways` şifreli — panelde **düz metin yok** |
| `ConnectionStrings`, Redis, RabbitMQ | `appsettings` / DevOps — admin salt okunur veya gizli |
| Müşteri adı, e-posta, adres | Üye işyeri DB — TriPay zorunlu tutmaz |

---

## 4. Admin modülleri → tablo eşlemesi

| Admin ekranı | Tablolar | Yazma |
| :--- | :--- | :---: |
| Dashboard | `Transactions` (aggregate), `OutboxMessages` | Hayır |
| İşlem listesi / detay | `Transactions`, `TransactionLogs` | Hayır |
| Üye işyerleri | `Merchants` | Aktif/pasif |
| Kanal listesi | `PaymentGateways` | Aktif/pasif |
| Gateway ayarları | `GatewaySettings` | Evet + cache invalidation |
| Hata sözlüğü | `GatewayErrorMappings` | Evet + cache invalidation |
| Outbox kuyruğu | `OutboxMessages` | Retry (ileri faz) |
| Kullanıcılar | `AspNetUsers`, roller | Evet (§17) |
| Redis cache | — | Temizle (operasyon) |

---

## 5. KVKK ve saklama politikası (Hosted)

| Ayar | `appsettings` | Etki |
| :--- | :--- | :--- |
| Hosted açık | `Persistence:Enabled: true` | `Transactions` yazılır |
| Ham log | `PersistTransactionLogs: true/false` | `TransactionLogs` |
| Webhook kuyruğu | `EnableOutbox: true/false` | `OutboxMessages` |

**Önerilen üretim profilleri:**

| Profil | Enabled | Logs | Outbox |
| :--- | :---: | :---: | :---: |
| Tam operasyon | true | true | true |
| KVKK hafif | true | **false** | true |
| Minimal | true | false | false |

Framework (NuGet) tüketicisi bu tabloların **hiçbirini** kullanmaz → TriPay tarafında KVKK işlem verisi riski **sıfıra yakın**.

---

## 6. Migration sırası (FluentMigrator)

| Sürüm | İçerik |
| :--- | :--- |
| `202605220001` | `Merchants`, `PaymentGateways`, `Transactions`, `TransactionLogs`, `OutboxMessages` |
| `202605220002` | Demo seed |
| `202605220003` | `GatewaySettings`, `GatewayErrorMappings` |
| `202605220004` | Vakıfbank metadata seed |
| `202605220010` (plan) | Identity tabloları |
| `202605220011` (plan) | Admin kullanıcı seed |

---

**Hazırlayan:** TriPay Geliştirme Ekibi

# TriPay — Proje Amacı, Kapsam ve Entegrasyon Modelleri

> **Program.cs:** [TriPay_Program_cs_ve_DI.md](./TriPay_Program_cs_ve_DI.md) · [TriPay_Admin_ve_Veritabani.md](./TriPay_Admin_ve_Veritabani.md)

**Versiyon:** 1.1 · **Tarih:** 22 Mayıs 2026 · **Kod durumu:** Framework/Hosted ayrımı uygulandı (`TriPay.Persistence`)

---

## 1. TriPay’in amacı

TriPay, Türkiye’deki sanal POS ve ödeme kuruluşu API’lerini tek .NET arayüzünde birleştiren **Payment Hub kütüphanesidir**. TriPay banka değildir; üye işyerinin kendi banka sözleşmesi ve credential’ları ile çalışır.

**İki ürün yüzü:**

| Yüz | Kim? | TriPay MSSQL |
| :--- | :--- | :---: |
| **Framework** | E-ticaret / entegrasyon developer | Hayır |
| **Hosted** | TriPay operatörü, demo site, SaaS | Evet |

---

## 2. En mantıklı seçim (risk minimizasyonu)

**Sizin hedefiniz (“hiçbir risk bizde kalmasın”) → Mod A: `AddTriPayFramework`**

| Neden | Açıklama |
| :--- | :--- |
| Veri sorumlusu | Müşteri verisi üye işyerinde kalır |
| TriPay log | `TransactionLogs` yazılmaz |
| Credential | Sizin Vault / appsettings — TriPay panelinde düz metin yok |
| NuGet bağımlılık | `TriPay.Services` — `TriPay.Data` çekilmez |

Hosted (Mod C) yalnızca TriPay’in **kendisi ödeme operatörü** olduğu senaryoda (tripay.com.tr, merkezi log, admin) kullanılır.

---

## 3. Kod yapısı (uygulandı)

```text
TriPay.Services       → AddTriPay() — provider'lar, IPaymentGatewayService
TriPay.Persistence    → AddTriPayFramework / AddTriPayHosted / AddTriPayPersistence
TriPay.Data           → MSSQL (Hosted)
TriPay.Infrastructure → Redis, RabbitMQ, gateway metadata cache
```

| Extension | `Persistence.Enabled` | Checkout |
| :--- | :---: | :---: |
| `AddTriPayFramework` | `false` (zorlar) | Hayır |
| `AddTriPayPersistence` | config’den | `true` ise evet |
| `AddTriPayHosted` | config’den (true) | Evet |

---

## 4. Yapılandırma bayrakları

```json
"TriPay": {
  "Persistence": {
    "Enabled": false,
    "PersistTransactionLogs": false,
    "EnableOutbox": false
  }
}
```

| Bayrak | `false` etkisi |
| :--- | :--- |
| `Enabled` | `IPaymentCheckoutService` DI’da yok |
| `PersistTransactionLogs` | `TransactionLogs` insert atlanır |
| `EnableOutbox` | `OutboxMessages` insert atlanır |

---

## 5. KVKK özeti

| Mod | TriPay kişisel veri riski |
| :--- | :--- |
| Framework | **Minimal** — yalnızca bankaya giden istek (sizin sorumluluğunuz) |
| Hosted + log | Orta — maskeli log, retention gerekir |
| Hosted C‑Lite | Düşük — özet işlem, ham log yok |

Detay: [TriPay_Admin_ve_Veritabani.md §5](./TriPay_Admin_ve_Veritabani.md#5-kvkk-ve-saklama-politikası-hosted)

---

## 6. Doküman haritası

| Ne arıyorsunuz? | Dosya |
| :--- | :--- |
| **`Program.cs` — hangi satırı yazacağım?** | [**TriPay_Program_cs_ve_DI.md**](./TriPay_Program_cs_ve_DI.md) |
| Framework appsettings / API | [TriPay_Framework_Modu.md](./TriPay_Framework_Modu.md) |
| Hosted DB / C‑Lite | [TriPay_Hosted_Modu.md](./TriPay_Hosted_Modu.md) |
| `AddTriPay()` (test / iç yapı) | [TriPay_AddTriPay_Dusuk_Seviye.md](./TriPay_AddTriPay_Dusuk_Seviye.md) |
| Ödeme API (Initialize, Callback, …) | [TriPay_Kullanim_Kilavuzu.md](./TriPay_Kullanim_Kilavuzu.md) |

---

**Hazırlayan:** TriPay Geliştirme Ekibi

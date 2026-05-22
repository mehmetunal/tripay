# TriPay — `AddTriPay()` (iç yapı / test)

> **Üretim `Program.cs` için bu dosyayı okumayın.**  
> Tek kaynak: [**TriPay_Program_cs_ve_DI.md**](./TriPay_Program_cs_ve_DI.md)

**Versiyon:** 1.1 · **Tarih:** 22 Mayıs 2026

---

## `AddTriPay` vs `AddTriPayFramework`

Tam karşılaştırma tablosu: [**TriPay_Program_cs_ve_DI.md §2**](./TriPay_Program_cs_ve_DI.md#2-addtripay-vs-addtripayframework--karşılaştırma)

---

## Ne?

`TriPay.Services` içindeki **parça** DI kaydı. Yalnızca:

- `VakifPays`, `Iyzico`, `Vakifbank` provider’ları  
- `IPaymentGatewayService`

**Kayıt etmez:** MSSQL, Redis, `IGatewaySettingsProvider`, checkout, outbox.

---

## Kim çağırır?

| Çağıran | Sizin yazmanız gerekir mi? |
| :--- | :---: |
| `AddTriPayFramework()` | Hayır — otomatik |
| `AddTriPayHosted()` | Hayır — otomatik |
| Unit test / özel host | Evet — bilinçli olarak |

---

## Eski Console örneği neden yanlıştı?

```csharp
// ❌ Üretimde kullanmayın — eksik Redis ve gateway settings
services.AddHttpClient();
services.AddTriPay();
```

Doğrusu: `services.AddTriPayFramework(context.Configuration);`  
Bkz. [TriPay_Program_cs_ve_DI.md §6](./TriPay_Program_cs_ve_DI.md#6-console--worker--framework)

---

## Elle Hosted yığını (yalnız ileri senaryo)

`AddTriPayHosted()` zaten şunu yapar; parça parça yazmak **aynı şeydir**:

`AddTriPayData` → `AddTriPayInfrastructure` → `AddTriPay` → `AddTriPayPersistence`

Detay: [TriPay_Program_cs_ve_DI.md §3](./TriPay_Program_cs_ve_DI.md#3-extension-hiyerarşisi-kafanız-karışmasın)

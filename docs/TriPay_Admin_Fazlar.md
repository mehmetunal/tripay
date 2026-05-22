# TriPay Admin — Faz planı (Tailwind)

> **Çalıştırma:** `dotnet run --project TriPay.Admin` → https://localhost:5055  
> **Varlıklar (Gulp):** `cd TriPay.Admin && npm install && npm run build` → `wwwroot/css/admin.min.css`, `wwwroot/js/admin.min.js`  
> **İzleme:** `npm run watch`

**UI:** Tailwind CSS 4 (demo ödeme sitesi `TriPay` Bootstrap kalır; yalnızca admin Tailwind kullanır).

---

## Durum: Admin panel tamamlandı ✅

Faz 1–3 tek sürümde uygulandı (işlemler, gateway CRUD, outbox, kullanıcılar, IP kısıtı).

---

## Faz 1 — Tamamlandı ✅

| Madde | Durum |
| :--- | :---: |
| `TriPay.Admin` projesi | ✅ |
| Gulp + Tailwind + **Trimango JS modülleri** (`wwwroot/js/modules`, `js-built/*.min.js`, jQuery + AJAX) | ✅ |
| Bootstrap / jQuery kaldırıldı | ✅ |
| FluentMigrator Identity tabloları (`202605220010`) | ✅ |
| `AddTriPayIdentity()` + cookie giriş | ✅ |
| Development seed: `admin@gmail.com` / `Super123!` | ✅ |
| Login / Logout / AccessDenied | ✅ |
| Dashboard özet (işlem, outbox, merchant, gateway sayıları) | ✅ |

---

## Faz 2 — Tamamlandı ✅

| Ekran | Controller |
| :--- | :--- |
| İşlem listesi + filtre | `TransactionsController` |
| İşlem detayı + maskeli log | `Transactions/Details` |
| Gateway ayarları CRUD | `GatewaysController` |
| Hata sözlüğü CRUD | `GatewaysController` |
| Redis önbellek temizle | `Gateways/ClearAllCache` |
| Outbox kuyruğu | `OutboxController` |
| Üye işyeri listesi + düzenle | `MerchantsController` |

---

## Faz 3 — Tamamlandı ✅

| Özellik | Not |
| :--- | :--- |
| Kullanıcı oluşturma / şifre sıfırlama / kilitleme | `UsersController` |
| Outbox yeniden kuyruk | `Outbox/Requeue` |
| Sistem (DB, Redis, migration, önbellek) | `SystemController` |
| IP kısıtı | `AdminIpRestrictionMiddleware` + `TriPay:Admin:AllowedIpRanges` |
| Development seed | `admin@gmail.com` / `Super123!` |

---

## Kapsam dışı

- Üye işyeri self-servis portalı  
- Banka credential (API key, şifre) panelden düzenleme  
- PAN / CVV görüntüleme  

Detay: [TriPay_Proje_Dokumani.md §17](./TriPay_Proje_Dokumani.md#17-yönetim-paneli-admin--en-son-faz)

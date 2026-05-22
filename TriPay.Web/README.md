# TriPay.Web

Kurumsal site ve kılavuz (`TriPay.Admin` ile aynı solution düzeni).

## URL modülleri

| Path | Area |
| :--- | :--- |
| `/` | (varsayılan) |
| `/docs` | Docs |
| `/pay` | Pay |
| `/admin` | Admin |

## Proje yapısı

```
TriPay.Web/
  Controllers/          ← Product, Solutions, Integration, Security, Corporate, Contact, Legal
  Views/
  Areas/
    Docs/Controllers + Views   ← Guide (kullanım kılavuzu)
    Pay/
    Admin/
```

Controller ve view klasör adları İngilizcedir; sayfa metinleri Türkçedir.

## Çalıştırma

```bash
cd TriPay.Web
npm install && npm run build
dotnet run
```

https://localhost:5200

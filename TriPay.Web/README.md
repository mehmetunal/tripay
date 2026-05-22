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
  Controllers/          ← kurumsal sayfalar
  Views/
  Areas/
    Docs/Controllers + Views
    Pay/
    Admin/
```

## Çalıştırma

```bash
cd TriPay.Web
npm install && npm run build
dotnet run
```

https://localhost:5200

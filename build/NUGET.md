# TriPay NuGet yayınlama

**Sürüm:** `build/TriPay.NuGet.Version.props` içindeki `TriPayPackageVersion` (tek kaynak).

Her `./build/pack-and-push.sh` (push) çalıştırmasında **patch** otomatik artar (`1.0.0` → `1.0.1`).  
`--pack-only` sürümü değiştirmez. Aynı sürümle push: `--no-bump`.  
Minor/major: `TRI_PAY_VERSION_BUMP=minor ./build/pack-and-push.sh`

**Hedef çerçeveler:** `net8.0`, `net9.0`, `net10.0` (tüm kütüphane ve meta paketler). Uygulamanız .NET 8 veya üzeri olmalıdır.

## Paketler (kullanıma göre)

| Paket | Kullanım |
| :--- | :--- |
| **TriPay.Framework** | `AddTriPayFramework` — kendi uygulamanız, TriPay DB yok (**önerilen**) |
| **TriPay.Hosted** | `AddTriPayHosted` — MSSQL + checkout + operatör |
| **TriPay** | Yalnızca `AddTriPay` — özel DI / test |
| TriPay.Core, TriPay.Data, TriPay.Infrastructure, TriPay.Persistence | Bağımlılık; doğrudan nadiren |

## Tüketici örneği

```bash
dotnet add package TriPay.Framework --version 1.0.0
```

```csharp
builder.Services.AddTriPayFramework(builder.Configuration);
```

## Yerel pack

```bash
chmod +x build/pack-and-push.sh
./build/pack-and-push.sh --pack-only
```

Çıktı: `artifacts/nupkgs/`

Her pakette:

- Kök **README.md** (nuget.org’da görünür) — web, GitHub, kılavuz linkleri  
- **docs/** klasörü — `TriPay_Kullanim_Kilavuzu.md`, `TriPay_Program_cs_ve_DI.md`, mod rehberleri, `INDEX.md`

## GitHub Actions

Repository secret: **`NUGET_API_KEY`** (nuget.org API anahtarı).

| Workflow | Ne zaman? | Ne yapar? |
| :--- | :--- | :--- |
| **CI** → `nuget-publish` job | `main` push (testler geçince) | Sürüm +1 → **pack** → **nuget.org push** → sürüm commit |
| **CI** → `nuget-pack-verify` | Pull request | Yalnızca pack doğrulama (**push yok**) |
| **NuGet Publish (manuel)** | Actions → Run workflow | İsteğe bağlı patch/minor/major |

Sürüm commit mesajındaki `[skip ci]` ile yalnızca `TriPay.NuGet.Version.props` güncellenir; bu commit için CI yeniden çalışmaz (sonsuz döngü önlenir).

## Yerel nuget.org push

```bash
export NUGET_API_KEY=...
./build/pack-and-push.sh
```

veya `/Users/mehmet/Project/maggsoft/nuget-api-key.txt` (tek satır, repoya eklemeyin).

## Yayın sırası

1. TriPay.Core  
2. TriPay (Services)  
3. TriPay.Data  
4. TriPay.Infrastructure  
5. TriPay.Persistence  
6. TriPay.Framework (meta)  
7. TriPay.Hosted (meta)  

Script bu sırayı kullanır.

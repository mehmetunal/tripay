namespace TriPay.Services.Providers.Vakifbank;

/// <summary>Vakıfbank hata kodları ve Türkçe açıklamaları (Trimango kaynağı).</summary>
public static class VakifbankErrorCodes
{
    private static readonly Dictionary<string, string> Codes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["0000"] = "Başarılı",
        ["0005"] = "İşlem onaylanmadı",
        ["0014"] = "Geçersiz kart numarası",
        ["0033"] = "Süresi bitmiş kart",
        ["0051"] = "Limit yetersiz",
        ["0055"] = "Hatalı kart şifresi",
        ["0580"] = "Hatalı 3D güvenlik bilgisi",
        ["0581"] = "ECI veya CAVV bilgisi eksik",
        ["1006"] = "Bu işlem daha önce başarılı gerçekleştirilmiş",
        ["1007"] = "Referans işlem alınamadı",
        ["1050"] = "CVV hatalı",
        ["1051"] = "Kredi kartı numarası hatalı",
        ["1052"] = "Kart son kullanma tarihi hatalı",
        ["5001"] = "İş yeri şifresi yanlış",
        ["5002"] = "İş yeri aktif değil"
    };

    /// <summary>Hata kodu için Türkçe açıklama döndürür; bilinmiyorsa null.</summary>
    public static string? GetDescription(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;
        return Codes.TryGetValue(code.Trim(), out var msg) ? msg : null;
    }
}

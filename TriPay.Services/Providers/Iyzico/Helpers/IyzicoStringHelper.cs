namespace TriPay.Services.Providers.Iyzico.Helpers;

/// <summary>Trimango StringNormalizationUtility karşılığı — Iyzico alıcı adı ayrıştırma.</summary>
public static class IyzicoStringHelper
{
    /// <summary>Tam adı ad ve soyad olarak böler.</summary>
    public static (string Name, string Surname) SplitFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return ("Musteri", "Adi");

        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            1 => (parts[0], "Adi"),
            _ => (parts[0], parts[1])
        };
    }
}

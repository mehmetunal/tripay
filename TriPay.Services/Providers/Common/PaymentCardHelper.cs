namespace TriPay.Services.Providers.Common;

/// <summary>Kart numarası ve son kullanma tarihi normalizasyon yardımcıları.</summary>
public static class PaymentCardHelper
{
    /// <summary>Metindeki yalnızca rakamları döndürür.</summary>
    public static string DigitsOnly(string input)
        => new((input ?? string.Empty).Where(char.IsDigit).ToArray());

    /// <summary>Son kullanma ayını iki haneli formata getirir.</summary>
    public static string NormalizeMonth(string value)
    {
        if (!int.TryParse(DigitsOnly(value), out var month) || month is < 1 or > 12)
            return "01";
        return month.ToString("00");
    }

    /// <summary>Son kullanma yılını dört haneli formata getirir.</summary>
    public static string NormalizeYear(string value)
    {
        var digits = DigitsOnly(value);
        if (digits.Length == 2) return "20" + digits;
        if (digits.Length == 4) return digits;
        return DateTime.UtcNow.Year.ToString();
    }
}

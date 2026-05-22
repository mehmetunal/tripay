using System.Globalization;
using Newtonsoft.Json;

namespace TriPay.Services.Providers.VakifPays.Helpers;

/// <summary>VakıfPayS form ve JSON yardımcıları.</summary>
public static class VakifPaysHttpHelper
{
    /// <summary>Decimal tutarı VakıfPayS formatına çevirir.</summary>
    public static string ToAmount(decimal amount)
        => amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")).Replace(".", "").Replace(",", ".");

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

    /// <summary>JSON yanıt gövdesini sözlüğe çevirir.</summary>
    public static Dictionary<string, object> ParseJsonDictionary(string raw)
        => JsonConvert.DeserializeObject<Dictionary<string, object>>(raw) ?? new Dictionary<string, object>();
}

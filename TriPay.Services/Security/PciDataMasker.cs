using System.Text.RegularExpressions;

namespace TriPay.Services.Security;

/// <summary>PCI-DSS: log ve DB payload'larında kart verisini maskeler.</summary>
public static partial class PciDataMasker
{
    /// <summary>JSON veya form metninde PAN, CVV ve benzeri alanları maskeler.</summary>
    public static string MaskSensitivePayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return string.Empty;

        var masked = payload;
        masked = PanRegex().Replace(masked, m => MaskPan(m.Value));
        masked = CvvRegex().Replace(masked, "$1***");
        masked = CardNumberJsonRegex().Replace(masked, "$1\"****\"");
        return masked;
    }

    /// <summary>Kart numarasının yalnızca son 4 hanesini gösterir.</summary>
    public static string MaskPan(string? pan)
    {
        if (string.IsNullOrWhiteSpace(pan))
            return string.Empty;

        var digits = new string(pan.Where(char.IsDigit).ToArray());
        if (digits.Length < 4)
            return "****";

        return new string('*', digits.Length - 4) + digits[^4..];
    }

    [GeneratedRegex(@"\b(?:\d[ -]*?){13,19}\b", RegexOptions.Compiled)]
    private static partial Regex PanRegex();

    [GeneratedRegex(@"(?i)(cvv|cvc|securityCode|cardCode)([""\s:=]+)([""']?)(\d{3,4})", RegexOptions.Compiled)]
    private static partial Regex CvvRegex();

    [GeneratedRegex(@"(?i)(""cardNumber""\s*:\s*)""[^""]+""", RegexOptions.Compiled)]
    private static partial Regex CardNumberJsonRegex();
}

using System.Globalization;

namespace TriPay.Services.Providers.Common;

/// <summary>Banka tutar formatlama yardımcıları.</summary>
public static class BankAmountHelper
{
    /// <summary>Türk formatında ondalıklı tutar (1234,56 → 1234.56).</summary>
    public static string FormatTurkishDecimal(decimal amount)
        => amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))
            .Replace(".", "")
            .Replace(",", ".");

    /// <summary>Garanti/YKB formatında kuruş tamsayısı (12.34 → 1234).</summary>
    public static string FormatMinorUnits(decimal amount)
        => amount.ToString("N2", CultureInfo.GetCultureInfo("en-US"))
            .Replace(",", "")
            .Replace(".", "");

    /// <summary>Kuveyt Türk / Vakıf Katılım formatında tutar (virgülsüz).</summary>
    public static string FormatCommaless(decimal amount)
        => amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))
            .Replace(".", "")
            .Replace(",", "");
}

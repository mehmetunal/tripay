using System.Security.Cryptography;
using System.Text;

namespace TriPay.Services.Providers.Nestpay.Helpers;

/// <summary>Nestpay EST 3D hash hesaplama yardımcıları.</summary>
public static class NestpayHashHelper
{
    /// <summary>ver3 hash algoritması ile SHA-512 hash üretir ve Base64 döndürür.</summary>
    public static string ComputeVer3Hash(IReadOnlyDictionary<string, string> parameters, string storeKey)
    {
        var ordered = parameters
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => EscapeHashValue(p.Value));

        var hashInput = string.Join("|", ordered) + "|" + storeKey;
        var hashBytes = SHA512.HashData(Encoding.UTF8.GetBytes(hashInput));
        return Convert.ToBase64String(hashBytes);
    }

    private static string EscapeHashValue(string value)
        => value.Replace("\\", "\\\\").Replace("|", "\\|");
}

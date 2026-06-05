using System.Security.Cryptography;
using System.Text;

namespace TriPay.Services.Providers.Common;

/// <summary>Banka sanal POS hash yardımcıları.</summary>
public static class BankHashHelper
{
    /// <summary>SHA-1 hash üretir ve Base64 döndürür.</summary>
    public static string Sha1Base64(string text)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(hash);
    }

    /// <summary>SHA-1 hash üretir ve büyük harf hex döndürür (Garanti vb.).</summary>
    public static string Sha1HexUpper(string text)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }

    /// <summary>SHA-256 hash üretir ve Base64 döndürür (Yapı Kredi vb.).</summary>
    public static string Sha256Base64(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(hash);
    }

    /// <summary>SHA-256 hash üretir ve küçük harf hex döndürür (Moka vb.).</summary>
    public static string Sha256HexLower(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>SHA-512 hash üretir ve Base64 döndürür (PayNKolay vb.).</summary>
    public static string Sha512Utf8Base64(string text)
    {
        var hash = SHA512.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(hash);
    }

    /// <summary>SHA-512 hash üretir ve Unicode metin ile büyük harf hex döndürür (Ahlpay vb.).</summary>
    public static string Sha512UnicodeHexUpper(string text)
    {
        var bytes = Encoding.Unicode.GetBytes(text);
        var hash = SHA512.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}

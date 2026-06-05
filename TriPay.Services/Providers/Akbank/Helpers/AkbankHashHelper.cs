using System.Security.Cryptography;
using System.Text;

namespace TriPay.Services.Providers.Akbank.Helpers;

/// <summary>Akbank sanal POS HMAC-SHA512 hash yardımcıları.</summary>
public static class AkbankHashHelper
{
    /// <summary>JSON gövdesi ve store key ile auth-hash üretir.</summary>
    public static string ComputeAuthHash(string jsonBody, string storeKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(storeKey);
        var messageBytes = Encoding.UTF8.GetBytes(jsonBody);
        var hash = HMACSHA512.HashData(keyBytes, messageBytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>3D form hash değerini üretir.</summary>
    public static string ComputeFormHash(string concatenatedFields, string storeKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(storeKey);
        var messageBytes = Encoding.UTF8.GetBytes(concatenatedFields);
        var hash = HMACSHA512.HashData(keyBytes, messageBytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>Güvenli rastgele hex dize üretir.</summary>
    public static string GenerateRandomHex(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var sb = new StringBuilder(length);
        foreach (var b in bytes)
            sb.Append((b % 16).ToString("X"));
        return sb.ToString();
    }
}

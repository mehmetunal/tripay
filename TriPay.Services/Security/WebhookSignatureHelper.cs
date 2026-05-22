using System.Security.Cryptography;
using System.Text;

namespace TriPay.Services.Security;

/// <summary>Üye işyeri webhook HMAC-SHA256 imza üretimi ve doğrulaması.</summary>
public static class WebhookSignatureHelper
{
    /// <summary>İmza için kabul edilen maksimum saat farkı.</summary>
    public static readonly TimeSpan TimestampTolerance = TimeSpan.FromMinutes(5);

    /// <summary>Payload + timestamp ile HMAC-SHA256 imza üretir (Base64).</summary>
    public static string ComputeSignature(string payload, string timestampUnix, string secret)
    {
        var data = payload + timestampUnix;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    /// <summary>Gelen imzanın geçerli ve zaman penceresi içinde olduğunu doğrular.</summary>
    public static bool Validate(string payload, string signature, string timestampUnix, string secret)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(timestampUnix))
            return false;

        if (!long.TryParse(timestampUnix, out var ts))
            return false;

        var eventTime = DateTimeOffset.FromUnixTimeSeconds(ts);
        if (Math.Abs((DateTimeOffset.UtcNow - eventTime).TotalMinutes) > TimestampTolerance.TotalMinutes)
            return false;

        var expected = ComputeSignature(payload, timestampUnix, secret);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.Trim()));
    }
}

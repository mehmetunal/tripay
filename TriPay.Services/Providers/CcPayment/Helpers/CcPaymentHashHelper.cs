using System.Security.Cryptography;
using System.Text;

namespace TriPay.Services.Providers.CcPayment.Helpers;

/// <summary>CCPayment (Sipay tipi) hash üretim ve doğrulama yardımcıları.</summary>
public static class CcPaymentHashHelper
{
    /// <summary>Satış isteği için hash_key üretir.</summary>
    public static string GenerateSaleHash(
        string total, string installment, string currencyCode, string merchantKey, string invoiceId, string appSecret)
        => GenerateHash($"{total}|{installment}|{currencyCode}|{merchantKey}|{invoiceId}", appSecret);

    /// <summary>3D tamamlama isteği için hash_key üretir.</summary>
    public static string GenerateCompleteHash(
        string merchantKey, string invoiceId, string orderId, string status, string appSecret)
        => GenerateHash($"{merchantKey}|{invoiceId}|{orderId}|{status}", appSecret);

    /// <summary>Callback hash_key değerini doğrular ve parçaları döndürür.</summary>
    public static IReadOnlyList<string> ValidateHash(string hashKey, string appSecret)
    {
        hashKey = hashKey.Replace("__", "/", StringComparison.Ordinal);
        var parts = hashKey.Split(':');
        if (parts.Length != 3)
            return Array.Empty<string>();

        var iv = parts[0];
        var salt = parts[1];
        var encrypted = parts[2];
        var password = Sha1Hex(appSecret);
        var saltWithPassword = Sha256Hex(password + salt)[..32];
        var decrypted = Decrypt(encrypted, saltWithPassword, iv);
        return decrypted.Split('|');
    }

    private static string GenerateHash(string text, string appSecret)
    {
        var iv = Sha1Hex(Random.Shared.Next().ToString())[..16];
        var password = Sha1Hex(appSecret);
        var salt = Sha1Hex(Random.Shared.Next().ToString())[..4];
        var saltWithPassword = Sha256Hex(password + salt)[..32];
        var encrypted = Encrypt(text, saltWithPassword, iv);
        return (iv + ":" + salt + ":" + encrypted).Replace("/", "__", StringComparison.Ordinal);
    }

    private static string Encrypt(string plainText, string key, string iv)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(iv);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var encryptor = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(encryptor.TransformFinalBlock(bytes, 0, bytes.Length));
    }

    private static string Decrypt(string cipherText, string key, string iv)
    {
        var encryptedBytes = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = Encoding.ASCII.GetBytes(key);
        aes.IV = Encoding.ASCII.GetBytes(iv);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.ASCII.GetString(decrypted);
    }

    private static string Sha1Hex(string input)
        => Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();

    private static string Sha256Hex(string input)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();
}

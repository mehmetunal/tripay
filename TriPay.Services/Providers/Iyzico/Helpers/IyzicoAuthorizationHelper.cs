using System.Security.Cryptography;
using System.Text;

namespace TriPay.Services.Providers.Iyzico.Helpers;

/// <summary>Iyzico IYZWSv2 authorization header üretim yardımcılarıdır.</summary>
public static class IyzicoAuthorizationHelper
{
    /// <summary>İstek gövdesi ve URI yoluna göre IYZWSv2 Authorization header değerini üretir.</summary>
    /// <param name="apiKey">Iyzico API anahtarı.</param>
    /// <param name="secretKey">Iyzico gizli anahtar.</param>
    /// <param name="uriPath">API yol parçası (ör. /payment/detail).</param>
    /// <param name="requestBodyJson">Serialize edilmiş JSON gövde; boş olabilir.</param>
    public static string Generate(string apiKey, string secretKey, string uriPath, string? requestBodyJson = null)
    {
        var randomKey = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var payload = string.IsNullOrEmpty(requestBodyJson) ? uriPath : uriPath + requestBodyJson;
        var dataToEncrypt = randomKey + payload;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToEncrypt)))
            .Replace("-", "", StringComparison.Ordinal)
            .ToLowerInvariant();

        var authorizationString = $"apiKey:{apiKey}&randomKey:{randomKey}&signature:{hash}";
        return "IYZWSv2 " + Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationString));
    }
}

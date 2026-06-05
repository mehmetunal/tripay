using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TriPay.Services.Providers.Tami.Helpers;

/// <summary>Tami JWK imza ve PG-Auth-Token yardımcıları.</summary>
public static class TamiJwkHelper
{
    /// <summary>PG-Auth-Token başlık değerini üretir.</summary>
    public static string BuildPgAuthToken(string merchantId, string merchantUser, string storeKey)
    {
        var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"{merchantId}{merchantUser}{storeKey}")));
        return $"{merchantId}:{merchantUser}:{hash}";
    }

    /// <summary>İstek gövdesi için HS512 JWK imzası üretir.</summary>
    public static string GenerateSecurityHash(string merchantPassword, Dictionary<string, object> requestBody)
    {
        var splitPassword = merchantPassword.Split('|');
        var kidValue = splitPassword[0];
        var kValue = splitPassword.Length > 1 ? splitPassword[1] : splitPassword[0];

        var bodyJson = JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        var headerObj = new { alg = "HS512", typ = "JWT", kidValue };
        var headerB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(headerObj)));
        var payloadB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(bodyJson));
        var signingInput = $"{headerB64}.{payloadB64}";
        var key = Convert.FromBase64String(NormalizeBase64Url(kValue));
        var signatureB64 = Convert.ToBase64String(
            HMACSHA512.HashData(key, Encoding.UTF8.GetBytes(signingInput)));

        return $"{headerB64}.{payloadB64}.{signatureB64}";
    }

    private static string NormalizeBase64Url(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        var remainder = base64.Length % 4;
        if (remainder == 2) return base64 + "==";
        if (remainder == 3) return base64 + "=";
        return base64;
    }
}

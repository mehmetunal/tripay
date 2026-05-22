namespace TriPay.Admin.Application;

/// <summary>API anahtarı maskeleme (sunum katmanı).</summary>
internal static class AdminApiKeyMasker
{
    public static string Mask(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 4)
            return "****";
        return new string('*', apiKey.Length - 4) + apiKey[^4..];
    }
}

namespace TriPay.Services.Providers.Nestpay;

/// <summary>Nestpay/EST sanal POS kanalı için canlı ve test ortamı endpoint adreslerini taşır.</summary>
public sealed record NestpayEndpointConfig(
    string LiveApiUrl,
    string Live3DUrl,
    string? TestApiUrl = null,
    string? Test3DUrl = null)
{
    /// <summary>Asseco genel test API adresi.</summary>
    public const string DefaultTestApiUrl = "https://entegrasyon.asseco-see.com.tr/fim/api";

    /// <summary>Asseco genel test 3D gateway adresi.</summary>
    public const string DefaultTest3DUrl = "https://entegrasyon.asseco-see.com.tr/fim/est3Dgate";

    /// <summary>Test modunda kullanılacak API URL'sini döndürür.</summary>
    public string ResolveApiUrl(bool isTestMode)
        => isTestMode ? (TestApiUrl ?? DefaultTestApiUrl) : LiveApiUrl;

    /// <summary>Test modunda kullanılacak 3D gateway URL'sini döndürür.</summary>
    public string Resolve3DUrl(bool isTestMode)
        => isTestMode ? (Test3DUrl ?? DefaultTest3DUrl) : Live3DUrl;
}

namespace TriPay.Services.Providers.Iyzico.Models;

/// <summary>Iyzico yapılandırmasından okunan API kimlik bilgileri ve doğrulama sonucunu taşır.</summary>
public sealed class IyzicoCredentialsResult
{
    /// <summary>Kimlik bilgilerinin kullanıma hazır olup olmadığını belirtir.</summary>
    public bool Ok { get; init; }

    /// <summary>Test (sandbox) ortamının seçilip seçilmediğini belirtir.</summary>
    public bool IsTestMode { get; init; }

    /// <summary>Iyzico API anahtarı.</summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>Iyzico gizli anahtar.</summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>Başarısız doğrulamada kullanıcıya veya loga yazılacak hata metni.</summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>Geçerli API anahtarları ile başarılı sonuç oluşturur.</summary>
    public static IyzicoCredentialsResult Success(bool isTestMode, string apiKey, string secretKey) => new()
    {
        Ok = true,
        IsTestMode = isTestMode,
        ApiKey = apiKey,
        SecretKey = secretKey
    };

    /// <summary>Eksik veya hatalı yapılandırma için başarısız sonuç oluşturur.</summary>
    public static IyzicoCredentialsResult Failure(string error, bool isTestMode = true, string apiKey = "", string secretKey = "") => new()
    {
        Ok = false,
        IsTestMode = isTestMode,
        ApiKey = apiKey,
        SecretKey = secretKey,
        Error = error
    };
}

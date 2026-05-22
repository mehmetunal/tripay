using TriPay.Core.Gateways;

namespace TriPay.Infrastructure.Gateways;

/// <summary>Test ortamı için bellek içi gateway metadata (DB/Redis yok).</summary>
public sealed class InMemoryGatewayMetadataService : IGatewayMetadataService
{
    private readonly Dictionary<string, Dictionary<string, string>> _settingsByGateway = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, string>> _errorsByGateway = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Vakıfbank varsayılan metadata ile oluşturur.</summary>
    public static InMemoryGatewayMetadataService CreateWithVakifbankDefaults()
    {
        var service = new InMemoryGatewayMetadataService();
        service._settingsByGateway["Vakifbank"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [GatewaySettingKeys.EnrollmentUrl] = "https://3dsecuretest.vakifbank.com.tr/MPIAPI/MPI_Enrollment.aspx",
            [GatewaySettingKeys.VerifyUrl] = "https://onlineodemetest.vakifbank.com.tr/VposService/v3/Vposreq.aspx",
            [GatewaySettingKeys.ResultCodeSuccess] = "0000",
            [GatewaySettingKeys.ThreeDsStatusEnrolled] = "Y",
            [GatewaySettingKeys.ThreeDsStatusAttempt] = "A",
            [GatewaySettingKeys.ThreeDsStatusNotEnrolled] = "N",
            [GatewaySettingKeys.ErrorCodeIssuerException] = "1001",
            [GatewaySettingKeys.NotEnrolledUserMessage] =
                "Kartınız 3D Secure ile doğrulanamadı veya bankanız işlemi kabul etmedi."
        };
        service._errorsByGateway["Vakifbank"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["0051"] = "Limit yetersiz",
            ["0000"] = "Başarılı"
        };
        return service;
    }

    /// <summary>Tek ayar değeri döner.</summary>
    public Task<string?> GetSettingAsync(string gatewayCode, string settingKey, bool isTestMode, CancellationToken cancellationToken = default)
    {
        if (!_settingsByGateway.TryGetValue(gatewayCode, out var dict))
            return Task.FromResult<string?>(null);
        return Task.FromResult(dict.TryGetValue(settingKey, out var v) ? v : null);
    }

    /// <summary>Hata mesajı döner.</summary>
    public Task<string?> GetErrorMessageAsync(string gatewayCode, string? providerErrorCode, string locale = "tr", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(providerErrorCode) || !_errorsByGateway.TryGetValue(gatewayCode, out var dict))
            return Task.FromResult<string?>(null);
        return Task.FromResult(dict.TryGetValue(providerErrorCode.Trim(), out var msg) ? msg : null);
    }

    /// <summary>Tüm ayarları döner.</summary>
    public Task<IReadOnlyDictionary<string, string>> GetSettingsAsync(string gatewayCode, bool isTestMode, CancellationToken cancellationToken = default)
    {
        if (_settingsByGateway.TryGetValue(gatewayCode, out var dict))
            return Task.FromResult<IReadOnlyDictionary<string, string>>(dict);
        return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>());
    }

    /// <summary>Tüm hata eşlemelerini döner.</summary>
    public Task<IReadOnlyDictionary<string, string>> GetErrorMapAsync(string gatewayCode, string locale = "tr", CancellationToken cancellationToken = default)
    {
        if (_errorsByGateway.TryGetValue(gatewayCode, out var dict))
            return Task.FromResult<IReadOnlyDictionary<string, string>>(dict);
        return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>());
    }
}

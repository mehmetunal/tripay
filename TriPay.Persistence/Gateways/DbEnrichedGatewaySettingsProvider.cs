using TriPay.Core.Gateways;
using TriPay.Infrastructure.Gateways;
using TriPay.Services.Configuration;

namespace TriPay.Persistence.Gateways;

/// <summary>
/// appsettings credential'ları + veritabanı URL/teknik ayarlarını birleştirir.
/// Provider'lar <see cref="IGatewaySettingsProvider"/> üzerinden tek kaynaktan okur.
/// </summary>
public sealed class DbEnrichedGatewaySettingsProvider : IGatewaySettingsProvider
{
    private readonly ConfigurationGatewaySettingsProvider _configProvider;
    private readonly IGatewayMetadataService _metadata;

    /// <summary>Birleşik ayar sağlayıcı oluşturur.</summary>
    public DbEnrichedGatewaySettingsProvider(
        ConfigurationGatewaySettingsProvider configProvider,
        IGatewayMetadataService metadata)
    {
        _configProvider = configProvider;
        _metadata = metadata;
    }

    /// <summary>DB metadata + appsettings birleşik yapılandırma döner.</summary>
    public async Task<GatewayConfig?> GetGatewayConfigAsync(string gatewayName, CancellationToken cancellationToken = default)
    {
        var config = await _configProvider.GetGatewayConfigAsync(gatewayName, cancellationToken);
        if (config == null)
            return null;

        var dbSettings = await _metadata.GetSettingsAsync(gatewayName, config.IsTestMode, cancellationToken);
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in dbSettings)
            merged[kv.Key] = kv.Value;

        foreach (var kv in config.Settings)
            merged[kv.Key] = kv.Value;

        return new GatewayConfig
        {
            Enabled = config.Enabled,
            IsTestMode = config.IsTestMode,
            Settings = merged
        };
    }
}

using Microsoft.Extensions.Options;
using TriPay.Core.Gateways;
using TriPay.Core.Options;

namespace TriPay.Services.Configuration;

/// <summary><see cref="TriPayOptions"/> üzerinden gateway ayarlarını okuyan yapılandırma sağlayıcısıdır.</summary>
public sealed class ConfigurationGatewaySettingsProvider : IGatewaySettingsProvider
{
    private readonly IOptionsMonitor<TriPayOptions> _options;

    /// <summary>IOptions monitor ile sağlayıcı örneği oluşturur.</summary>
    public ConfigurationGatewaySettingsProvider(IOptionsMonitor<TriPayOptions> options)
    {
        _options = options;
    }

    /// <summary>appsettings içindeki <c>TriPay:Gateways</c> bölümünden kanal yapılandırmasını okur.</summary>
    public Task<GatewayConfig?> GetGatewayConfigAsync(string gatewayName, CancellationToken cancellationToken = default)
    {
        var gateways = _options.CurrentValue.Gateways;
        if (!gateways.TryGetValue(gatewayName, out var config) || !config.Enabled)
            return Task.FromResult<GatewayConfig?>(null);

        return Task.FromResult<GatewayConfig?>(config);
    }
}

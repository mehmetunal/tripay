using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Interfaces;

namespace TriPay.Tests.Fixtures;

/// <summary>Testlerde gateway yapılandırmasını bellekten sağlar.</summary>
public sealed class FakeGatewaySettings : IGatewaySettingsProvider
{
    private readonly GatewayConfig _config;

    /// <summary>Belirtilen gateway için sahte ayar oluşturur.</summary>
    public FakeGatewaySettings(string gatewayName, Dictionary<string, string> settings, bool enabled = true)
    {
        _config = new GatewayConfig
        {
            Enabled = enabled,
            IsTestMode = true,
            Settings = settings
        };
        GatewayName = gatewayName;
    }

    /// <summary>Sahte provider gateway adı.</summary>
    public string GatewayName { get; }

    /// <summary>Test gateway adı eşleşirse sahte yapılandırmayı döndürür.</summary>
    public Task<GatewayConfig?> GetGatewayConfigAsync(string gatewayName, CancellationToken cancellationToken = default)
        => Task.FromResult<GatewayConfig?>(
            string.Equals(gatewayName, GatewayName, StringComparison.OrdinalIgnoreCase) ? _config : null);
}

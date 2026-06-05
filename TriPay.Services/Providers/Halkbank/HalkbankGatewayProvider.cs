using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.Halkbank;

/// <summary>Halkbank Nestpay sanal POS entegrasyonu.</summary>
public sealed class HalkbankGatewayProvider : NestpayGatewayBase
{
    /// <summary>Halkbank provider örneği oluşturur.</summary>
    public HalkbankGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<HalkbankGatewayProvider> logger)
        : base(NestpayEndpoints.Halkbank, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.Halkbank;

    /// <inheritdoc />
    public override string DisplayName => "Halkbank";
}

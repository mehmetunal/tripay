using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.Ziraat;

/// <summary>Ziraat Bankası Nestpay sanal POS entegrasyonu.</summary>
public sealed class ZiraatGatewayProvider : NestpayGatewayBase
{
    /// <summary>Ziraat Bankası provider örneği oluşturur.</summary>
    public ZiraatGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<ZiraatGatewayProvider> logger)
        : base(NestpayEndpoints.Ziraat, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.Ziraat;

    /// <inheritdoc />
    public override string DisplayName => "Ziraat Bankası";
}

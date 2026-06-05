using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.ING;

/// <summary>ING Bank Nestpay sanal POS entegrasyonu.</summary>
public sealed class INGGatewayProvider : NestpayGatewayBase
{
    /// <summary>ING Bank provider örneği oluşturur.</summary>
    public INGGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<INGGatewayProvider> logger)
        : base(NestpayEndpoints.ING, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.ING;

    /// <inheritdoc />
    public override string DisplayName => "ING Bank";
}

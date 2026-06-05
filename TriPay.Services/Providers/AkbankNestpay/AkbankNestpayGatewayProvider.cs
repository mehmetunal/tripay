using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.AkbankNestpay;

/// <summary>Akbank Nestpay (EST) sanal POS entegrasyonu.</summary>
public sealed class AkbankNestpayGatewayProvider : NestpayGatewayBase
{
    /// <summary>Akbank Nestpay provider örneği oluşturur.</summary>
    public AkbankNestpayGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<AkbankNestpayGatewayProvider> logger)
        : base(NestpayEndpoints.AkbankNestpay, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.AkbankNestpay;

    /// <inheritdoc />
    public override string DisplayName => "Akbank Nestpay";
}

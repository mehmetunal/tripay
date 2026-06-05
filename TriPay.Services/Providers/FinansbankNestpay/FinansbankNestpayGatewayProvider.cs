using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.FinansbankNestpay;

/// <summary>QNB Finansbank Nestpay (eski Finansbank EST) sanal POS entegrasyonu.</summary>
public sealed class FinansbankNestpayGatewayProvider : NestpayGatewayBase
{
    /// <summary>Finansbank Nestpay provider örneği oluşturur.</summary>
    public FinansbankNestpayGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<FinansbankNestpayGatewayProvider> logger)
        : base(NestpayEndpoints.FinansbankNestpay, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.FinansbankNestpay;

    /// <inheritdoc />
    public override string DisplayName => "Finansbank Nestpay";
}

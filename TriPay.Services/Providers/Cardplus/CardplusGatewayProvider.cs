using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.Cardplus;

/// <summary>Cardplus Nestpay sanal POS entegrasyonu.</summary>
public sealed class CardplusGatewayProvider : NestpayGatewayBase
{
    /// <summary>Cardplus provider örneği oluşturur.</summary>
    public CardplusGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<CardplusGatewayProvider> logger)
        : base(NestpayEndpoints.Cardplus, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.Cardplus;

    /// <inheritdoc />
    public override string DisplayName => "Cardplus";
}

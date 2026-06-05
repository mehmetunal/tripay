using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.Anadolubank;

/// <summary>Anadolubank Nestpay sanal POS entegrasyonu.</summary>
public sealed class AnadolubankGatewayProvider : NestpayGatewayBase
{
    /// <summary>Anadolubank provider örneği oluşturur.</summary>
    public AnadolubankGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<AnadolubankGatewayProvider> logger)
        : base(NestpayEndpoints.Anadolubank, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.Anadolubank;

    /// <inheritdoc />
    public override string DisplayName => "Anadolubank";
}

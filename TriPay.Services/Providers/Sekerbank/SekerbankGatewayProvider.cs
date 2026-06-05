using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.Sekerbank;

/// <summary>Şekerbank Nestpay sanal POS entegrasyonu.</summary>
public sealed class SekerbankGatewayProvider : NestpayGatewayBase
{
    /// <summary>Şekerbank provider örneği oluşturur.</summary>
    public SekerbankGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<SekerbankGatewayProvider> logger)
        : base(NestpayEndpoints.Sekerbank, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.Sekerbank;

    /// <inheritdoc />
    public override string DisplayName => "Şekerbank";
}

using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.TurkiyeFinans;

/// <summary>Türkiye Finans Nestpay sanal POS entegrasyonu.</summary>
public sealed class TurkiyeFinansGatewayProvider : NestpayGatewayBase
{
    /// <summary>Türkiye Finans provider örneği oluşturur.</summary>
    public TurkiyeFinansGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<TurkiyeFinansGatewayProvider> logger)
        : base(NestpayEndpoints.TurkiyeFinans, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.TurkiyeFinans;

    /// <inheritdoc />
    public override string DisplayName => "Türkiye Finans";
}

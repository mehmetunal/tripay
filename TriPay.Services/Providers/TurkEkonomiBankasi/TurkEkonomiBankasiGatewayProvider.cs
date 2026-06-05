using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.TurkEkonomiBankasi;

/// <summary>Türk Ekonomi Bankası Nestpay sanal POS entegrasyonu.</summary>
public sealed class TurkEkonomiBankasiGatewayProvider : NestpayGatewayBase
{
    /// <summary>TEB provider örneği oluşturur.</summary>
    public TurkEkonomiBankasiGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<TurkEkonomiBankasiGatewayProvider> logger)
        : base(NestpayEndpoints.TurkEkonomiBankasi, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.TurkEkonomiBankasi;

    /// <inheritdoc />
    public override string DisplayName => "Türk Ekonomi Bankası";
}

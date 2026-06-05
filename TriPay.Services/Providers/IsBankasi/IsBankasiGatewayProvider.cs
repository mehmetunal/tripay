using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.IsBankasi;

/// <summary>İş Bankası Nestpay sanal POS entegrasyonu.</summary>
public sealed class IsBankasiGatewayProvider : NestpayGatewayBase
{
    /// <summary>İş Bankası provider örneği oluşturur.</summary>
    public IsBankasiGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<IsBankasiGatewayProvider> logger)
        : base(NestpayEndpoints.IsBankasi, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.IsBankasi;

    /// <inheritdoc />
    public override string DisplayName => "İş Bankası";
}

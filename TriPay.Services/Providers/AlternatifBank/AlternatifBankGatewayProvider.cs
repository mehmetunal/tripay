using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Nestpay;

namespace TriPay.Services.Providers.AlternatifBank;

/// <summary>Alternatif Bank Nestpay sanal POS entegrasyonu.</summary>
public sealed class AlternatifBankGatewayProvider : NestpayGatewayBase
{
    /// <summary>Alternatif Bank provider örneği oluşturur.</summary>
    public AlternatifBankGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<AlternatifBankGatewayProvider> logger)
        : base(NestpayEndpoints.AlternatifBank, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.AlternatifBank;

    /// <inheritdoc />
    public override string DisplayName => "Alternatif Bank";
}

using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Protocols.ApiV2;

namespace TriPay.Services.Providers.PaytenMsu;

/// <summary>Payten MSU (Merchant Safe Unipay) sanal POS entegrasyonu.</summary>
public sealed class PaytenMsuGatewayProvider : ApiV2ProtocolServerSide3DGatewayBase
{
    public PaytenMsuGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<PaytenMsuGatewayProvider> logger)
        : base(ApiV2Endpoints.PaytenMsu, settingsProvider, httpClientFactory, logger)
    {
    }

    public override string GatewayName => PaymentGatewayNames.PaytenMsu;
    public override string DisplayName => "Payten MSU";
}

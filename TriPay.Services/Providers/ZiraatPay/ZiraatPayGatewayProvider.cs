using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Protocols.ApiV2;

namespace TriPay.Services.Providers.ZiraatPay;

/// <summary>ZiraatPay API v2 sanal POS entegrasyonu.</summary>
public sealed class ZiraatPayGatewayProvider : ApiV2ProtocolServerSide3DGatewayBase
{
    public ZiraatPayGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<ZiraatPayGatewayProvider> logger)
        : base(ApiV2Endpoints.ZiraatPay, settingsProvider, httpClientFactory, logger)
    {
    }

    public override string GatewayName => PaymentGatewayNames.ZiraatPay;
    public override string DisplayName => "ZiraatPay";
}

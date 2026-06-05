using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Protocols.ApiV2;

namespace TriPay.Services.Providers.Paratika;

/// <summary>Paratika API v2 sanal POS entegrasyonu.</summary>
public sealed class ParatikaGatewayProvider : ApiV2ProtocolServerSide3DGatewayBase
{
    public ParatikaGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<ParatikaGatewayProvider> logger)
        : base(ApiV2Endpoints.Paratika, settingsProvider, httpClientFactory, logger)
    {
    }

    public override string GatewayName => PaymentGatewayNames.Paratika;
    public override string DisplayName => "Paratika";
}

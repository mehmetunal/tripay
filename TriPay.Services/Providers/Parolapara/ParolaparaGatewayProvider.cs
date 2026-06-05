using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.CcPayment;

namespace TriPay.Services.Providers.Parolapara;

/// <summary>Parolapara CCPayment sanal POS entegrasyonu.</summary>
public sealed class ParolaparaGatewayProvider : CcPaymentGatewayBase
{
    public ParolaparaGatewayProvider(IGatewaySettingsProvider s, IHttpClientFactory h, ILogger<ParolaparaGatewayProvider> l)
        : base(CcPaymentEndpoints.Parolapara, s, h, l) { }
    public override string GatewayName => PaymentGatewayNames.Parolapara;
    public override string DisplayName => "Parolapara";
}

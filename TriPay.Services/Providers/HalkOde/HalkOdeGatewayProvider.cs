using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.CcPayment;

namespace TriPay.Services.Providers.HalkOde;

/// <summary>HalkÖde CCPayment sanal POS entegrasyonu.</summary>
public sealed class HalkOdeGatewayProvider : CcPaymentGatewayBase
{
    public HalkOdeGatewayProvider(IGatewaySettingsProvider s, IHttpClientFactory h, ILogger<HalkOdeGatewayProvider> l)
        : base(CcPaymentEndpoints.HalkOde, s, h, l) { }
    public override string GatewayName => PaymentGatewayNames.HalkOde;
    public override string DisplayName => "HalkÖde";
}

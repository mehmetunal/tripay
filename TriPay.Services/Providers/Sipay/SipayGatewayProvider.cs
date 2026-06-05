using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.CcPayment;

namespace TriPay.Services.Providers.Sipay;

/// <summary>Sipay CCPayment sanal POS entegrasyonu.</summary>
public sealed class SipayGatewayProvider : CcPaymentGatewayBase
{
    public SipayGatewayProvider(IGatewaySettingsProvider s, IHttpClientFactory h, ILogger<SipayGatewayProvider> l)
        : base(CcPaymentEndpoints.Sipay, s, h, l) { }
    public override string GatewayName => PaymentGatewayNames.Sipay;
    public override string DisplayName => "Sipay";
}

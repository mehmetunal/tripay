using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.CcPayment;

namespace TriPay.Services.Providers.PayBull;

/// <summary>PayBull CCPayment sanal POS entegrasyonu.</summary>
public sealed class PayBullGatewayProvider : CcPaymentGatewayBase
{
    public PayBullGatewayProvider(IGatewaySettingsProvider s, IHttpClientFactory h, ILogger<PayBullGatewayProvider> l)
        : base(CcPaymentEndpoints.PayBull, s, h, l) { }
    public override string GatewayName => PaymentGatewayNames.PayBull;
    public override string DisplayName => "PayBull";
}

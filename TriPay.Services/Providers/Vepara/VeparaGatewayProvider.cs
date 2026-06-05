using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.CcPayment;

namespace TriPay.Services.Providers.Vepara;

/// <summary>Vepara CCPayment sanal POS entegrasyonu.</summary>
public sealed class VeparaGatewayProvider : CcPaymentGatewayBase
{
    public VeparaGatewayProvider(IGatewaySettingsProvider s, IHttpClientFactory h, ILogger<VeparaGatewayProvider> l)
        : base(CcPaymentEndpoints.Vepara, s, h, l) { }
    public override string GatewayName => PaymentGatewayNames.Vepara;
    public override string DisplayName => "Vepara";
}

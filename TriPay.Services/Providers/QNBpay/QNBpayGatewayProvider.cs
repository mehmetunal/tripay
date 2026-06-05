using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.CcPayment;

namespace TriPay.Services.Providers.QNBpay;

/// <summary>QNBpay CCPayment sanal POS entegrasyonu.</summary>
public sealed class QNBpayGatewayProvider : CcPaymentGatewayBase
{
    public QNBpayGatewayProvider(IGatewaySettingsProvider s, IHttpClientFactory h, ILogger<QNBpayGatewayProvider> l)
        : base(CcPaymentEndpoints.QNBpay, s, h, l) { }
    public override string GatewayName => PaymentGatewayNames.QNBpay;
    public override string DisplayName => "QNBpay";
}

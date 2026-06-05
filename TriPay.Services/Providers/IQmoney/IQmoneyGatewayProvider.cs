using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.CcPayment;

namespace TriPay.Services.Providers.IQmoney;

/// <summary>IQmoney CCPayment sanal POS entegrasyonu.</summary>
public sealed class IQmoneyGatewayProvider : CcPaymentGatewayBase
{
    public IQmoneyGatewayProvider(IGatewaySettingsProvider s, IHttpClientFactory h, ILogger<IQmoneyGatewayProvider> l)
        : base(CcPaymentEndpoints.IQmoney, s, h, l) { }
    public override string GatewayName => PaymentGatewayNames.IQmoney;
    public override string DisplayName => "IQmoney";
}

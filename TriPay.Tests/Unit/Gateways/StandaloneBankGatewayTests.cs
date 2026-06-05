using TriPay.Core.Gateways;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.Gateways;

/// <summary>Doğrudan HttpPaymentGatewayBase kullanan banka ve kuruluş testleri.</summary>
public sealed class StandaloneBankGatewayTests
{
    public static IEnumerable<object[]> StandaloneGateways()
        => GatewayProviderTestCatalog.All
            .Where(x => x.Protocol is GatewayProtocolKind.MdStatusBank
                or GatewayProtocolKind.ParamPos
                or GatewayProtocolKind.Moka
                or GatewayProtocolKind.PayNKolay
                or GatewayProtocolKind.Paynet
                or GatewayProtocolKind.Ahlpay
                or GatewayProtocolKind.Tami)
            .Select(x => new object[] { x.GatewayName });

    [Theory]
    [InlineData(PaymentGatewayNames.Garanti)]
    [InlineData(PaymentGatewayNames.Akbank)]
    [InlineData(PaymentGatewayNames.Denizbank)]
    [InlineData(PaymentGatewayNames.YapiKredi)]
    [InlineData(PaymentGatewayNames.KuveytTurk)]
    [InlineData(PaymentGatewayNames.VakifKatilim)]
    [InlineData(PaymentGatewayNames.QNBFinansbank)]
    public async Task MdStatusBank_InitializePayment_Basarili(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        var result = await provider.InitializePaymentAsync(GatewayProviderTestFactory.CreateInitializeRequest());
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(PaymentGatewayNames.Tami)]
    [InlineData(PaymentGatewayNames.Moka)]
    [InlineData(PaymentGatewayNames.Paynet)]
    [InlineData(PaymentGatewayNames.PayNKolay)]
    public async Task OdemeKurulusu_InitializePayment_Basarili(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        var result = await provider.InitializePaymentAsync(GatewayProviderTestFactory.CreateInitializeRequest());
        Assert.True(result.IsSuccess);
    }
}

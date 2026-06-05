using TriPay.Core.Gateways;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.Gateways;

/// <summary>CCPayment protokolünü kullanan ödeme kuruluşları için testler.</summary>
public sealed class CcPaymentProtocolTests
{
    public static IEnumerable<object[]> CcPaymentGateways()
        => GatewayProviderTestCatalog.All
            .Where(x => x.Protocol == GatewayProtocolKind.CcPayment)
            .Select(x => new object[] { x.GatewayName });

    [Theory]
    [MemberData(nameof(CcPaymentGateways))]
    public async Task InitializePayment_3D_RedirectHtml_Dondurur(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        var result = await provider.InitializePaymentAsync(GatewayProviderTestFactory.CreateInitializeRequest());

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Data!.RedirectHtml));
    }

    [Theory]
    [MemberData(nameof(CcPaymentGateways))]
    public async Task ProcessCallback_MdStatus1_Basarili(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        var result = await provider.ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto
        {
            RawData = new Dictionary<string, string>
            {
                ["md_status"] = "1",
                ["invoice_id"] = "INV-1"
            }
        });

        Assert.True(result.IsSuccess);
        Assert.True(result.Data!.Success);
    }

    [Fact]
    public void CcPaymentKurulus_Sayisi_7()
    {
        var count = GatewayProviderTestCatalog.All.Count(x => x.Protocol == GatewayProtocolKind.CcPayment);
        Assert.Equal(7, count);
    }
}

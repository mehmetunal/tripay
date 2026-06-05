using TriPay.Core.Gateways;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.Gateways;

/// <summary>Nestpay protokolünü kullanan bankalar için testler.</summary>
public sealed class NestpayProtocolTests
{
    public static IEnumerable<object[]> NestpayGateways()
        => GatewayProviderTestCatalog.All
            .Where(x => x.Protocol == GatewayProtocolKind.Nestpay)
            .Select(x => new object[] { x.GatewayName });

    [Theory]
    [MemberData(nameof(NestpayGateways))]
    public async Task InitializePayment_3D_RedirectHtml_Dondurur(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        var result = await provider.InitializePaymentAsync(GatewayProviderTestFactory.CreateInitializeRequest());

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Data!.RedirectHtml));
    }

    [Theory]
    [MemberData(nameof(NestpayGateways))]
    public async Task Auth3DS_BosCallback_HataDondurur(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        var result = await provider.Auth3DSAsync(new PaymentGatewayAuth3DSRequestDto
        {
            RawData = new Dictionary<string, string>()
        });

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void NestpayBanka_Sayisi_12()
    {
        var count = GatewayProviderTestCatalog.All.Count(x => x.Protocol == GatewayProtocolKind.Nestpay);
        Assert.Equal(12, count);
    }
}

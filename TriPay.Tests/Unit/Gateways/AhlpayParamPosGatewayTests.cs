using TriPay.Core.Gateways;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.Gateways;

/// <summary>Ahlpay ve ParamPos özel callback / ayar testleri.</summary>
public sealed class AhlpayParamPosGatewayTests
{
    [Fact]
    public async Task Ahlpay_ProcessCallback_Basarili()
    {
        var provider = GatewayProviderTestFactory.Create(PaymentGatewayNames.Ahlpay);
        var result = await provider.ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto
        {
            RawData = new Dictionary<string, string> { ["orderId"] = "ORDER-TEST-1" }
        });

        Assert.True(result.IsSuccess);
        Assert.True(result.Data!.Success);
        Assert.Equal("ORDER-TEST-1", result.Data.OrderNumber);
    }

    [Fact]
    public async Task ParamPos_ProcessCallback_Basarili()
    {
        var provider = GatewayProviderTestFactory.Create(PaymentGatewayNames.ParamPos);
        var result = await provider.ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto
        {
            RawData = new Dictionary<string, string>
            {
                ["mdStatus"] = "1",
                ["orderId"] = "ORDER-TEST-1"
            }
        });

        Assert.True(result.IsSuccess);
        Assert.True(result.Data!.Success);
    }

    [Fact]
    public async Task Ahlpay_IsSupported_AktifAyar_ile_True()
    {
        var provider = GatewayProviderTestFactory.Create(PaymentGatewayNames.Ahlpay);
        Assert.True(await provider.IsSupportedAsync());
    }

    [Fact]
    public async Task ParamPos_IsSupported_AktifAyar_ile_True()
    {
        var provider = GatewayProviderTestFactory.Create(PaymentGatewayNames.ParamPos);
        Assert.True(await provider.IsSupportedAsync());
    }
}

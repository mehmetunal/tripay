using TriPay.Services;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit;

/// <summary><see cref="PaymentGatewayFactory"/> unit testleri.</summary>
public sealed class PaymentGatewayFactoryTests
{
    [Fact]
    public void GetAllAvailableGateways_UcMvpProvider()
    {
        using var sp = TestServiceProviderFactory.CreatePaymentServices();
        var factory = sp.GetRequiredService<PaymentGatewayFactory>();
        var names = factory.GetAllAvailableGateways();

        Assert.Contains(PaymentGatewayNames.VakifPays, names);
        Assert.Contains(PaymentGatewayNames.Iyzico, names);
        Assert.Contains(PaymentGatewayNames.Vakifbank, names);
    }

    [Fact]
    public async Task GetGatewayProviderAsync_BilinmeyenGateway_Null()
    {
        using var sp = TestServiceProviderFactory.CreatePaymentServices();
        var factory = sp.GetRequiredService<PaymentGatewayFactory>();
        var provider = await factory.GetGatewayProviderAsync("BilinmeyenBanka");
        Assert.Null(provider);
    }

    [Fact]
    public async Task GetGatewayProviderAsync_VakifPays_ProviderDondurur()
    {
        using var sp = TestServiceProviderFactory.CreatePaymentServices();
        var factory = sp.GetRequiredService<PaymentGatewayFactory>();
        var provider = await factory.GetGatewayProviderAsync(PaymentGatewayNames.VakifPays);
        Assert.NotNull(provider);
        Assert.Equal(PaymentGatewayNames.VakifPays, provider!.GatewayName);
    }

    [Fact]
    public void GetSystemActiveGatewayNames_AktifProviderlar()
    {
        using var sp = TestServiceProviderFactory.CreatePaymentServices();
        var factory = sp.GetRequiredService<PaymentGatewayFactory>();
        var active = factory.GetSystemActiveGatewayNames();
        Assert.NotEmpty(active);
    }
}

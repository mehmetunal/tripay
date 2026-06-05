using Microsoft.Extensions.DependencyInjection;
using TriPay.Core.Gateways;
using TriPay.Services;
using TriPay.Services.DependencyInjection;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit;

/// <summary><see cref="PaymentGatewayFactory"/> unit testleri.</summary>
public sealed class PaymentGatewayFactoryTests
{
    [Fact]
    public void GetAllAvailableGateways_38Provider()
    {
        using var sp = CreateFullFactoryServices();
        var factory = sp.GetRequiredService<PaymentGatewayFactory>();
        var names = factory.GetAllAvailableGateways();

        Assert.Equal(38, names.Count);
        Assert.Contains(PaymentGatewayNames.VakifPays, names);
        Assert.Contains(PaymentGatewayNames.Iyzico, names);
        Assert.Contains(PaymentGatewayNames.Vakifbank, names);
        Assert.Contains(PaymentGatewayNames.Paratika, names);
        Assert.Contains(PaymentGatewayNames.Sipay, names);
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
        using var sp = CreateFullFactoryServices();
        var factory = sp.GetRequiredService<PaymentGatewayFactory>();
        var active = factory.GetSystemActiveGatewayNames();
        Assert.Equal(38, active.Count);
    }

    [Theory]
    [InlineData(PaymentGatewayNames.Garanti)]
    [InlineData(PaymentGatewayNames.Paratika)]
    [InlineData(PaymentGatewayNames.IsBankasi)]
    [InlineData(PaymentGatewayNames.Sipay)]
    public async Task GetGatewayProviderAsync_KayitliGateway_ProviderDondurur(string gatewayName)
    {
        using var sp = CreateFullFactoryServices(configure: services =>
        {
            services.Configure<TriPay.Core.Options.TriPayOptions>(o =>
            {
                o.Gateways = GatewayProviderTestCatalog.All.ToDictionary(
                    x => x.GatewayName,
                    x => new GatewayConfig
                    {
                        Enabled = true,
                        IsTestMode = true,
                        Settings = new Dictionary<string, string>(x.Settings)
                    },
                    StringComparer.OrdinalIgnoreCase);
            });
        });

        var factory = sp.GetRequiredService<PaymentGatewayFactory>();
        var provider = await factory.GetGatewayProviderAsync(gatewayName);
        Assert.NotNull(provider);
        Assert.Equal(gatewayName, provider!.GatewayName, StringComparer.OrdinalIgnoreCase);
    }

    private static ServiceProvider CreateFullFactoryServices(Action<IServiceCollection>? configure = null)
    {
        return TestServiceProviderFactory.CreatePaymentServices(services =>
        {
            services.AddPaymentGatewayProviders();
            configure?.Invoke(services);
        });
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TriPay.Core.Vakifbank;
using TriPay.Core.Gateways;
using TriPay.Infrastructure.DependencyInjection;
using TriPay.Infrastructure.Gateways;
using TriPay.Services;
using TriPay.Services.Checkout;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Interfaces;
using TriPay.Services.Providers.Iyzico;
using TriPay.Services.Providers.Vakifbank;
using TriPay.Services.Providers.VakifPays;

namespace TriPay.Tests.Fixtures;

/// <summary>Integration testler için minimal DI konteyneri oluşturur.</summary>
public static class TestServiceProviderFactory
{
    /// <summary>VakıfPayS, Iyzico ve Vakıfbank provider'ları ile tam ödeme servisi kurar.</summary>
    public static ServiceProvider CreatePaymentServices(
        Action<IServiceCollection>? configure = null,
        IHttpClientFactory? httpClientFactory = null,
        IVakifbankSaleStateStore? saleStateStore = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TriPay:Redis:Enabled"] = "false",
                ["ConnectionStrings:Redis"] = "localhost:6379"
            })
            .Build();

        services.AddTriPayRedis(config);

        services.Configure<TriPayOptions>(o =>
        {
            o.DefaultGateway = PaymentGatewayNames.VakifPays;
            o.Gateways = new Dictionary<string, GatewayConfig>(StringComparer.OrdinalIgnoreCase)
            {
                [PaymentGatewayNames.VakifPays] = new()
                {
                    Enabled = true,
                    IsTestMode = true,
                    Settings = new Dictionary<string, string>
                    {
                        ["Merchant"] = "10009011",
                        ["MerchantUser"] = "apitest48@vakifpays.com.tr",
                        ["MerchantPassword"] = "Api.123.1234"
                    }
                },
                [PaymentGatewayNames.Iyzico] = new()
                {
                    Enabled = true,
                    IsTestMode = true,
                    Settings = new Dictionary<string, string>
                    {
                        ["ApiKey"] = "key",
                        ["SecretKey"] = "secret"
                    }
                },
                [PaymentGatewayNames.Vakifbank] = new()
                {
                    Enabled = true,
                    IsTestMode = true,
                    Settings = new Dictionary<string, string>
                    {
                        ["MerchantId"] = "m",
                        ["MerchantPassword"] = "p",
                        ["TerminalNo"] = "t",
                        ["InstallmentCounts"] = "3,6",
                        ["BinPrefixes"] = "493841"
                    }
                }
            };
        });

        if (saleStateStore != null)
            services.AddSingleton(saleStateStore);

        services.AddSingleton<ConfigurationGatewaySettingsProvider>();
        services.AddScoped<IGatewayMetadataService>(_ => InMemoryGatewayMetadataService.CreateWithVakifbankDefaults());
        services.AddScoped<IGatewaySettingsProvider, DbEnrichedGatewaySettingsProvider>();
        services.AddSingleton(httpClientFactory ?? VakifPaysTestHttp.CreateClientFactory());

        services.AddSingleton<PaymentGatewayFactory>();
        services.AddScoped<PaymentGatewayService>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddScoped<VakifPaysGatewayProvider>();
        services.AddScoped<IyzicoGatewayProvider>();
        services.AddScoped<VakifbankGatewayProvider>();

        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }
}

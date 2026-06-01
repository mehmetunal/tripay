using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Infrastructure.DependencyInjection;
using TriPay.Infrastructure.Gateways;
using TriPay.Services.Configuration;
using TriPay.Services.Interfaces;
using TriPay.Services.Providers.Iyzico;
using TriPay.Services.Providers.Vakifbank;
using TriPay.Services.Providers.VakifPays;

namespace TriPay.Services.DependencyInjection;

/// <summary>TriPay ödeme servisleri ve provider'larının DI kayıt extension metodlarını içerir.</summary>
public static class PaymentGatewayServiceCollectionExtensions
{
    /// <summary>
    /// Motor parçası: provider'lar ve <see cref="IPaymentGatewayService"/>.
    /// Üretim için <c>AddTriPayFramework</c> veya <c>AddTriPayHosted</c> kullanın.
    /// </summary>
    public static IServiceCollection AddTriPay(this IServiceCollection services, IConfiguration? configuration = null)
    {
        if (configuration != null)
        {
            services.Configure<TriPayOptions>(configuration.GetSection("TriPay"));
            services.Configure<TriPayPersistenceOptions>(
                configuration.GetSection(TriPayPersistenceOptions.SectionName));
        }

        services.AddHttpClient();

        services.AddScoped<PaymentGatewayFactory>();
        services.AddScoped<VakifPaysGatewayProvider>();
        services.AddScoped<IyzicoGatewayProvider>();
        services.AddScoped<VakifbankGatewayProvider>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        
        services.AddSingleton<ConfigurationGatewaySettingsProvider>();

        return services;
    }

    /// <summary>
    /// Framework modu: provider'lar + Redis; TriPay MSSQL ve checkout <strong>yok</strong>.
    /// Banka bilgileri üye işyeri <c>appsettings</c> / Vault'tan gelir.
    /// </summary>
    public static IServiceCollection AddTriPayFramework(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTriPay(configuration);
        services.AddTriPayRedis(configuration);
        
        // Vakıfbank provider ctor; Hosted MSSQL metadata olmadan çözülemez — bellek içi varsayılanlar.
        services.AddSingleton<IGatewayMetadataService>(_ => InMemoryGatewayMetadataService.CreateWithVakifbankDefaults());
        
        services.AddSingleton<IGatewaySettingsProvider>(sp =>
            sp.GetRequiredService<ConfigurationGatewaySettingsProvider>());
            
        services.Configure<TriPayPersistenceOptions>(o =>
        {
            o.Enabled = false;
            o.PersistTransactionLogs = false;
            o.EnableOutbox = false;
        });
        
        return services;
    }

    /// <summary>Eski extension adı; <see cref="AddTriPay"/> kullanın.</summary>
    public static IServiceCollection AddTriPayPaymentGateways(this IServiceCollection services)
        => services.AddTriPay();
}

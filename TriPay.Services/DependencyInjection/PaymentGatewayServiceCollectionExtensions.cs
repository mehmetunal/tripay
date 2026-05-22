using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    /// Framework modu: yalnızca provider'lar ve <see cref="IPaymentGatewayService"/>.
    /// TriPay MSSQL / checkout için <c>AddTriPayPersistence</c> veya <c>AddTriPayHosted</c> kullanın.
    /// </summary>
    public static IServiceCollection AddTriPay(this IServiceCollection services, IConfiguration? configuration = null)
    {
        if (configuration != null)
        {
            services.Configure<TriPayOptions>(configuration.GetSection("TriPay"));
            services.Configure<TriPay.Core.Options.TriPayPersistenceOptions>(
                configuration.GetSection(TriPay.Core.Options.TriPayPersistenceOptions.SectionName));
        }

        services.AddHttpClient();

        services.AddScoped<PaymentGatewayFactory>();
        services.AddScoped<VakifPaysGatewayProvider>();
        services.AddScoped<IyzicoGatewayProvider>();
        services.AddScoped<VakifbankGatewayProvider>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();

        return services;
    }

    /// <summary>Eski extension adı; <see cref="AddTriPay"/> kullanın.</summary>
    public static IServiceCollection AddTriPayPaymentGateways(this IServiceCollection services)
        => services.AddTriPay();
}

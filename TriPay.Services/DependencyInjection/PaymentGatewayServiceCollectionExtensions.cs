using Microsoft.Extensions.DependencyInjection;
using TriPay.Services.Interfaces;
using TriPay.Services.Providers;

namespace TriPay.Services.DependencyInjection;

public static class PaymentGatewayServiceCollectionExtensions
{
    public static IServiceCollection AddTriPay(this IServiceCollection services)
    {
        services.AddHttpClient<VakifPaysService>();
        services.AddScoped<PaymentGatewayFactory>();
        services.AddScoped<VakifPaysGatewayProvider>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();

        return services;
    }

    /// <summary>Eski extension adı; <see cref="AddTriPay"/> kullanın.</summary>
    public static IServiceCollection AddTriPayPaymentGateways(this IServiceCollection services)
        => services.AddTriPay();
}

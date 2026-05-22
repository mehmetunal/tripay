using Microsoft.Extensions.DependencyInjection;
using TriPay.Services.PaymentGateways;
using TriPay.Services.PaymentGateways.Interfaces;
using TriPay.Services.PaymentGateways.Providers;
using TriPay.Services.PaymentGateways.Services;

namespace TriPay.Services.DependencyInjection;

public static class PaymentGatewayServiceCollectionExtensions
{
    public static IServiceCollection AddTriPayPaymentGateways(this IServiceCollection services)
    {
        services.AddHttpClient<VakifPaysService>();
        services.AddScoped<PaymentGatewayFactory>();
        services.AddScoped<VakifPaysGatewayProvider>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();

        return services;
    }
}

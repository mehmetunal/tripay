using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Persistence.Checkout;
using TriPay.Persistence.Gateways;
using TriPay.Services.Checkout;

namespace TriPay.Persistence.DependencyInjection;

/// <summary>Hosted mod: MSSQL işlem, opsiyonel log ve checkout.</summary>
public static class TriPayPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// TriPay veritabanı kalıcılığı ve <see cref="IPaymentCheckoutService"/> kaydı.
    /// <c>TriPay:Persistence:Enabled=true</c> olmalıdır.
    /// </summary>
    public static IServiceCollection AddTriPayPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TriPayPersistenceOptions>(
            configuration.GetSection(TriPayPersistenceOptions.SectionName));

        var persistence = configuration.GetSection(TriPayPersistenceOptions.SectionName).Get<TriPayPersistenceOptions>()
            ?? new TriPayPersistenceOptions();

        if (persistence.Enabled)
        {
            services.AddScoped<IPaymentCheckoutService, PaymentCheckoutService>();
            
            // Hosted modda credential'lar DB metadata ile birleştirilir.
            services.AddScoped<IGatewaySettingsProvider, DbEnrichedGatewaySettingsProvider>();
        }

        return services;
    }
}

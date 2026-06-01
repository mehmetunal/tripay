using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriPay.Data.DependencyInjection;
using TriPay.Infrastructure.DependencyInjection;
using TriPay.Services.DependencyInjection;

namespace TriPay.Persistence.DependencyInjection;

/// <summary>TriPay Hosted (SaaS / demo web) tam yığın kaydı.</summary>
public static class TriPayHostedServiceCollectionExtensions
{
    /// <summary>
    /// Framework + MSSQL + Redis metadata + checkout + outbox.
    /// TriPay operatörü için; KVKK log riski yapılandırma ile sınırlanır.
    /// </summary>
    public static IServiceCollection AddTriPayHosted(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTriPayData(configuration);
        services.AddTriPayInfrastructure(configuration);
        services.AddTriPay(configuration);
        services.AddTriPayPersistence(configuration);
        return services;
    }
}

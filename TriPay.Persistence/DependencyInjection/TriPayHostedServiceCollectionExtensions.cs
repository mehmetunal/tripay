using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriPay.Core.Options;
using TriPay.Data.DependencyInjection;
using TriPay.Infrastructure.DependencyInjection;
using TriPay.Services.Configuration;
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

    /// <summary>
    /// Framework modu: provider'lar + Redis; TriPay MSSQL ve checkout <strong>yok</strong>.
    /// Banka bilgileri üye işyeri <c>appsettings</c> / Vault'tan gelir.
    /// </summary>
    public static IServiceCollection AddTriPayFramework(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTriPay(configuration);
        services.AddTriPayRedis(configuration);
        services.AddSingleton<ConfigurationGatewaySettingsProvider>();
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
}

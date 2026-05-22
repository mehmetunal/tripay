using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriPay.Core.Gateways;
using TriPay.Infrastructure.Configuration;
using TriPay.Infrastructure.Gateways;
using TriPay.Infrastructure.Messaging;
using TriPay.Infrastructure.Workers;
using TriPay.Services.Configuration;

namespace TriPay.Infrastructure.DependencyInjection;

/// <summary>TriPay.Infrastructure — Redis + RabbitMQ kayıtları.</summary>
public static class TriPayInfrastructureServiceCollectionExtensions
{
    /// <summary>Redis, gateway metadata, RabbitMQ outbox ve worker'ları kaydeder.</summary>
    public static IServiceCollection AddTriPayInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTriPayRedis(configuration);

        services.AddSingleton<ConfigurationGatewaySettingsProvider>();
        services.AddScoped<IGatewayMetadataService, RedisCachedGatewayMetadataService>();
        services.AddScoped<IGatewaySettingsProvider, DbEnrichedGatewaySettingsProvider>();

        services.Configure<TriPayRabbitMqOptions>(configuration.GetSection(TriPayRabbitMqOptions.SectionName));
        services.AddSingleton<RabbitMqOutboxPublisher>();
        services.AddHostedService<OutboxDispatcherHostedService>();
        return services;
    }
}

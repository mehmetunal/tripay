using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriPay.Core.Gateways;
using TriPay.Infrastructure.Configuration;
using TriPay.Infrastructure.Gateways;
using TriPay.Infrastructure.Messaging;
using TriPay.Infrastructure.Workers;

namespace TriPay.Infrastructure.DependencyInjection;

/// <summary>TriPay.Infrastructure — Redis + RabbitMQ kayıtları.</summary>
public static class TriPayInfrastructureServiceCollectionExtensions
{
    /// <summary>Redis, gateway metadata, RabbitMQ outbox ve worker'ları kaydeder.</summary>
    public static IServiceCollection AddTriPayInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTriPayRedis(configuration);

        services.AddScoped<IGatewayMetadataService, RedisCachedGatewayMetadataService>();

        services.Configure<TriPayRabbitMqOptions>(configuration.GetSection(TriPayRabbitMqOptions.SectionName));
        services.AddSingleton<RabbitMqOutboxPublisher>();
        services.AddHostedService<OutboxDispatcherHostedService>();
        return services;
    }
}

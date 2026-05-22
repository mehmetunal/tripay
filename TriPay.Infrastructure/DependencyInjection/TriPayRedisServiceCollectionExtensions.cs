using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TriPay.Core.Idempotency;
using TriPay.Core.Options;
using TriPay.Core.Redis;
using TriPay.Core.Vakifbank;
using TriPay.Infrastructure.Redis;

namespace TriPay.Infrastructure.DependencyInjection;

/// <summary>TriPay Redis altyapısı DI kayıtları (tüm proje).</summary>
public static class TriPayRedisServiceCollectionExtensions
{
    /// <summary>Redis önbellek, idempotency, 3D state, kilit ve rate limit kaydı.</summary>
    public static IServiceCollection AddTriPayRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(TriPayRedisOptions.SectionName);
        services.Configure<TriPayRedisOptions>(section);

        var redisOptions = new TriPayRedisOptions();
        section.Bind(redisOptions);

        var connection = !string.IsNullOrWhiteSpace(redisOptions.Configuration)
            ? redisOptions.Configuration
            : configuration.GetConnectionString("Redis") ?? "localhost:6379";

        var instanceName = string.IsNullOrWhiteSpace(redisOptions.InstanceName) ? "tripay:" : redisOptions.InstanceName;

        if (redisOptions.Enabled)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connection;
                options.InstanceName = instanceName;
            });
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connection));
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ITriPayRedisCache>(sp => new TriPayRedisCache(
            sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>(),
            sp.GetService<IConnectionMultiplexer>()));

        if (redisOptions.Enabled)
        {
            services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
            services.AddSingleton<IVakifbankSaleStateStore, RedisVakifbankSaleStateStore>();
            services.AddSingleton<IRedisDistributedLock, RedisDistributedLock>();
            services.AddSingleton<IRedisRateLimiter, RedisRateLimiter>();
        }
        else
        {
            services.AddSingleton<IIdempotencyStore, NullIdempotencyStore>();
            services.AddSingleton<IVakifbankSaleStateStore, InMemoryVakifbankSaleStateStore>();
            services.AddSingleton<IRedisDistributedLock, NoOpDistributedLock>();
            services.AddSingleton<IRedisRateLimiter, AllowAllRateLimiter>();
        }

        return services;
    }

    private sealed class NoOpDistributedLock : IRedisDistributedLock
    {
        public Task<IAsyncDisposable?> TryAcquireAsync(string resourceKey, CancellationToken cancellationToken = default)
            => Task.FromResult<IAsyncDisposable?>(new AsyncEmptyDisposable());
    }

    private sealed class AllowAllRateLimiter : IRedisRateLimiter
    {
        public Task<bool> AllowAsync(int merchantId, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class AsyncEmptyDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

using Microsoft.Extensions.Options;
using TriPay.Core.Options;
using TriPay.Core.Redis;

namespace TriPay.Infrastructure.Redis;

/// <summary><see cref="IRedisRateLimiter"/> — sabit pencere sayaç.</summary>
public sealed class RedisRateLimiter : IRedisRateLimiter
{
    private readonly ITriPayRedisCache _redis;
    private readonly TriPayRedisOptions _options;

    /// <summary>Redis sayaç ile rate limiter oluşturur.</summary>
    public RedisRateLimiter(ITriPayRedisCache redis, IOptions<TriPayRedisOptions> options)
    {
        _redis = redis;
        _options = options.Value;
    }

    /// <summary>İstek izin verildi mi kontrol eder.</summary>
    public async Task<bool> AllowAsync(int merchantId, CancellationToken cancellationToken = default)
    {
        var key = RedisKeyNames.RateLimit(merchantId);
        var current = await _redis.GetAsync<int?>(key, cancellationToken) ?? 0;
        if (current >= _options.RateLimitMaxRequests)
            return false;

        await _redis.SetAsync(key, current + 1, TimeSpan.FromSeconds(_options.RateLimitWindowSeconds), cancellationToken);
        return true;
    }
}

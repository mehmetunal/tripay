using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TriPay.Core.Options;
using TriPay.Core.Redis;

namespace TriPay.Infrastructure.Redis;

/// <summary><see cref="ITriPayRedisCache"/> — <see cref="IDistributedCache"/> + doğrudan Redis ping.</summary>
public sealed class TriPayRedisCache : ITriPayRedisCache
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _multiplexer;

    /// <summary>Önbellek ve isteğe bağlı çoklu sunucu bağlantısı ile oluşturur.</summary>
    public TriPayRedisCache(IDistributedCache cache, IConnectionMultiplexer? multiplexer = null)
    {
        _cache = cache;
        _multiplexer = multiplexer;
    }

    /// <summary>JSON değer okur.</summary>
    public async Task<T?> GetAsync<T>(string logicalKey, CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(logicalKey, cancellationToken);
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    /// <summary>JSON değer yazar.</summary>
    public Task SetAsync<T>(string logicalKey, T value, TimeSpan? absoluteExpiration, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        var options = new DistributedCacheEntryOptions();
        if (absoluteExpiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = absoluteExpiration;

        return _cache.SetStringAsync(logicalKey, json, options, cancellationToken);
    }

    /// <summary>Ham bayt okur.</summary>
    public Task<byte[]?> GetBytesAsync(string logicalKey, CancellationToken cancellationToken = default)
        => _cache.GetAsync(logicalKey, cancellationToken);

    /// <summary>Ham bayt yazar.</summary>
    public Task SetBytesAsync(string logicalKey, byte[] value, TimeSpan? absoluteExpiration, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (absoluteExpiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = absoluteExpiration;

        return _cache.SetAsync(logicalKey, value, options, cancellationToken);
    }

    /// <summary>Anahtarı siler.</summary>
    public Task RemoveAsync(string logicalKey, CancellationToken cancellationToken = default)
        => _cache.RemoveAsync(logicalKey, cancellationToken);

    /// <summary>Redis bağlantısı canlı mı kontrol eder.</summary>
    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        if (_multiplexer == null || !_multiplexer.IsConnected)
            return true;

        try
        {
            var db = _multiplexer.GetDatabase();
            _ = await db.PingAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

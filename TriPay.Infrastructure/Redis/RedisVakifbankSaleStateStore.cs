using Microsoft.Extensions.Options;
using TriPay.Core.Options;
using TriPay.Core.Redis;
using TriPay.Core.Vakifbank;

namespace TriPay.Infrastructure.Redis;

/// <summary><see cref="IVakifbankSaleStateStore"/> Redis uygulaması.</summary>
public sealed class RedisVakifbankSaleStateStore : IVakifbankSaleStateStore
{
    private readonly ITriPayRedisCache _redis;
    private readonly TriPayRedisOptions _options;

    /// <summary>Redis önbellek ile depo oluşturur.</summary>
    public RedisVakifbankSaleStateStore(ITriPayRedisCache redis, IOptions<TriPayRedisOptions> options)
    {
        _redis = redis;
        _options = options.Value;
    }

    /// <summary>3D sonrası satış durumunu Redis'e yazar.</summary>
    public Task SetAsync(string orderCode, VakifbankSaleState state, CancellationToken cancellationToken = default)
    {
        var ttl = TimeSpan.FromHours(Math.Max(1, _options.SaleStateTtlHours));
        return _redis.SetAsync(RedisKeyNames.VakifbankSale(orderCode), state, ttl, cancellationToken);
    }

    /// <summary>Sipariş koduna göre satış durumunu okur.</summary>
    public Task<VakifbankSaleState?> GetAsync(string orderCode, CancellationToken cancellationToken = default)
        => _redis.GetAsync<VakifbankSaleState>(RedisKeyNames.VakifbankSale(orderCode), cancellationToken);

    /// <summary>Auth3DS sonrası kaydı siler.</summary>
    public Task RemoveAsync(string orderCode, CancellationToken cancellationToken = default)
        => _redis.RemoveAsync(RedisKeyNames.VakifbankSale(orderCode), cancellationToken);
}

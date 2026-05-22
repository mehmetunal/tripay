using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TriPay.Core.Common;
using TriPay.Core.Idempotency;
using TriPay.Core.Options;
using TriPay.Infrastructure.Redis;
using TriPay.Services.Models;

namespace TriPay.Tests.Unit.Idempotency;

/// <summary>Redis idempotency store unit testleri (bellek içi distributed cache).</summary>
public sealed class RedisIdempotencyStoreTests
{
    [Fact]
    public async Task SaveVeTryGet_AyniSonucuDondurur()
    {
        var store = CreateStore();
        var key = "test-callback-1";
        var expected = Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = true,
            OrderNumber = "ORD-1"
        });

        await store.SaveProcessedAsync(key, expected);
        var cached = await store.TryGetProcessedAsync<PaymentGatewayCallbackResponseDto>(key);

        Assert.NotNull(cached);
        Assert.True(cached!.IsSuccess);
        Assert.Equal("ORD-1", cached.Data!.OrderNumber);
    }

    [Fact]
    public async Task TryGet_OlmayanAnahtar_NullDondurur()
    {
        var store = CreateStore();
        var cached = await store.TryGetProcessedAsync<PaymentGatewayCallbackResponseDto>("yok");
        Assert.Null(cached);
    }

    private static RedisIdempotencyStore CreateStore()
    {
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        services.Configure<TriPayRedisOptions>(o => o.IdempotencyTtlDays = 1);
        var sp = services.BuildServiceProvider();
        var cache = new TriPayRedisCache(sp.GetRequiredService<IDistributedCache>(), null);
        return new RedisIdempotencyStore(cache, sp.GetRequiredService<IOptions<TriPayRedisOptions>>());
    }
}

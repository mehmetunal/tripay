using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TriPay.Core.Options;
using TriPay.Core.Redis;

namespace TriPay.Infrastructure.Redis;

/// <summary><see cref="IRedisDistributedLock"/> — Redis SET NX EX.</summary>
public sealed class RedisDistributedLock : IRedisDistributedLock
{
    private readonly IConnectionMultiplexer? _multiplexer;
    private readonly TriPayRedisOptions _options;

    /// <summary>Redis bağlantısı ile kilit servisi oluşturur.</summary>
    public RedisDistributedLock(IConnectionMultiplexer? multiplexer, IOptions<TriPayRedisOptions> options)
    {
        _multiplexer = multiplexer;
        _options = options.Value;
    }

    /// <summary>Kilit almayı dener.</summary>
    public async Task<IAsyncDisposable?> TryAcquireAsync(string resourceKey, CancellationToken cancellationToken = default)
    {
        if (_multiplexer == null || !_multiplexer.IsConnected)
            return new NoOpLock();

        var db = _multiplexer.GetDatabase();
        var token = Guid.NewGuid().ToString("N");
        var ttl = TimeSpan.FromSeconds(Math.Max(5, _options.DistributedLockSeconds));
        var acquired = await db.StringSetAsync(resourceKey, token, ttl, When.NotExists);

        return acquired ? new RedisLockHandle(db, resourceKey, token) : null;
    }

    private sealed class RedisLockHandle : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _token;

        public RedisLockHandle(IDatabase db, string key, string token)
        {
            _db = db;
            _key = key;
            _token = token;
        }

        public async ValueTask DisposeAsync()
        {
            const string script = """
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                end
                return 0
                """;
            await _db.ScriptEvaluateAsync(script, [_key], [_token]);
        }
    }

    private sealed class NoOpLock : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

using System.Text.Json;
using Microsoft.Extensions.Options;
using TriPay.Core.Common;
using TriPay.Core.Idempotency;
using TriPay.Core.Options;
using TriPay.Core.Redis;

namespace TriPay.Infrastructure.Redis;

/// <summary><see cref="IIdempotencyStore"/> Redis uygulaması.</summary>
public sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly ITriPayRedisCache _redis;
    private readonly TriPayRedisOptions _options;

    /// <summary>Redis önbellek ve seçenekler ile store oluşturur.</summary>
    public RedisIdempotencyStore(ITriPayRedisCache redis, IOptions<TriPayRedisOptions> options)
    {
        _redis = redis;
        _options = options.Value;
    }

    /// <summary>Daha önce işlenmiş sonucu okur.</summary>
    public async Task<Result<T>?> TryGetProcessedAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await _redis.GetBytesAsync(RedisKeyNames.Idempotency(key), cancellationToken);
        if (bytes == null || bytes.Length == 0)
            return null;

        var payload = JsonSerializer.Deserialize<IdempotencyPayload>(bytes);
        if (payload == null)
            return null;

        T? data = default;
        if (!string.IsNullOrWhiteSpace(payload.DataJson) && !string.IsNullOrWhiteSpace(payload.DataType))
        {
            var type = Type.GetType(payload.DataType, throwOnError: false);
            if (type != null)
                data = (T?)JsonSerializer.Deserialize(payload.DataJson, type);
        }

        return payload.IsSuccess
            ? Result<T>.Success(data!)
            : Result<T>.Failure(payload.ErrorMessage ?? "Önceden işlenmiş başarısız istek.", data);
    }

    /// <summary>İşlem sonucunu TTL ile kaydeder.</summary>
    public async Task SaveProcessedAsync<T>(string key, Result<T> result, CancellationToken cancellationToken = default)
    {
        var payload = new IdempotencyPayload
        {
            IsSuccess = result.IsSuccess,
            ErrorMessage = result.ErrorMessage,
            DataType = typeof(T).AssemblyQualifiedName,
            DataJson = result.Data == null ? null : JsonSerializer.Serialize(result.Data)
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var ttl = TimeSpan.FromDays(Math.Max(1, _options.IdempotencyTtlDays));
        await _redis.SetBytesAsync(RedisKeyNames.Idempotency(key), bytes, ttl, cancellationToken);
    }
}

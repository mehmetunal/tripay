using TriPay.Core.Common;
using TriPay.Core.Idempotency;

namespace TriPay.Infrastructure.Redis;

/// <summary>Redis devre dışıyken idempotency uygulanmaz (yalnızca test / yerel).</summary>
public sealed class NullIdempotencyStore : IIdempotencyStore
{
    /// <summary>Her zaman null döner.</summary>
    public Task<Result<T>?> TryGetProcessedAsync<T>(string key, CancellationToken cancellationToken = default)
        => Task.FromResult<Result<T>?>(null);

    /// <summary>Hiçbir şey kaydetmez.</summary>
    public Task SaveProcessedAsync<T>(string key, Result<T> result, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

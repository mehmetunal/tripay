namespace TriPay.Core.Redis;

/// <summary>Paralel callback / işlem yarışını önleyen dağıtık kilit.</summary>
public interface IRedisDistributedLock
{
    /// <summary>Kilit almayı dener; başarılıysa <paramref name="handle"/> döner.</summary>
    Task<IAsyncDisposable?> TryAcquireAsync(string resourceKey, CancellationToken cancellationToken = default);
}

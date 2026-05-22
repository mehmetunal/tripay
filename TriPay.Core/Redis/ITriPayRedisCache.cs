namespace TriPay.Core.Redis;

/// <summary>TriPay genel Redis önbellek sözleşmesi (tüm modüller bu arayüzü kullanır).</summary>
public interface ITriPayRedisCache
{
    /// <summary>JSON değer okur.</summary>
    Task<T?> GetAsync<T>(string logicalKey, CancellationToken cancellationToken = default);

    /// <summary>JSON değer yazar.</summary>
    Task SetAsync<T>(string logicalKey, T value, TimeSpan? absoluteExpiration, CancellationToken cancellationToken = default);

    /// <summary>Ham bayt okur.</summary>
    Task<byte[]?> GetBytesAsync(string logicalKey, CancellationToken cancellationToken = default);

    /// <summary>Ham bayt yazar.</summary>
    Task SetBytesAsync(string logicalKey, byte[] value, TimeSpan? absoluteExpiration, CancellationToken cancellationToken = default);

    /// <summary>Anahtarı siler.</summary>
    Task RemoveAsync(string logicalKey, CancellationToken cancellationToken = default);

    /// <summary>Redis bağlantısı canlı mı kontrol eder.</summary>
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
}

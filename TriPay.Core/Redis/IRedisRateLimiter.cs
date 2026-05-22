namespace TriPay.Core.Redis;

/// <summary>Üye işyeri / API rate limit (Redis sayaç).</summary>
public interface IRedisRateLimiter
{
    /// <summary>İstek izin verildi mi kontrol eder ve sayacı artırır.</summary>
    Task<bool> AllowAsync(int merchantId, CancellationToken cancellationToken = default);
}

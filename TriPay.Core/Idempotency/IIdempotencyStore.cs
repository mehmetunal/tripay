using TriPay.Core.Common;

namespace TriPay.Core.Idempotency;

/// <summary>Callback ve Auth3DS tekrarlarını engelleyen idempotency deposu.</summary>
public interface IIdempotencyStore
{
    /// <summary>Daha önce işlenmiş sonucu okur.</summary>
    Task<Result<T>?> TryGetProcessedAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>İşlem sonucunu kalıcı olarak kaydeder.</summary>
    Task SaveProcessedAsync<T>(string key, Result<T> result, CancellationToken cancellationToken = default);
}

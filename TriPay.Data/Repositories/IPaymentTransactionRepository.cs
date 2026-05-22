using TriPay.Data.Entities;

namespace TriPay.Data.Repositories;

/// <summary>Ödeme işlem kayıtları için veri erişimi.</summary>
public interface IPaymentTransactionRepository
{
    /// <summary>Kanal koduna göre gateway kimliğini döner.</summary>
    Task<int?> GetGatewayIdByCodeAsync(string gatewayCode, CancellationToken cancellationToken = default);

    /// <summary>Demo üye işyeri kimliğini döner (ilk aktif kayıt).</summary>
    Task<int> GetDefaultMerchantIdAsync(CancellationToken cancellationToken = default);

    /// <summary>Üye işyeri ve sipariş numarasına göre işlem getirir.</summary>
    Task<PaymentTransaction?> GetByOrderAsync(int merchantId, string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>Yeni işlem ekler.</summary>
    Task<PaymentTransaction> AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>İşlem kaydını günceller.</summary>
    Task UpdateAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>İşlem logu ekler.</summary>
    Task AddLogAsync(PaymentTransactionLog log, CancellationToken cancellationToken = default);

    /// <summary>Outbox mesajı ekler.</summary>
    Task AddOutboxAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>Yayınlanmamış outbox mesajlarını getirir.</summary>
    Task<IReadOnlyList<OutboxMessage>> GetPendingOutboxAsync(int take, CancellationToken cancellationToken = default);

    /// <summary>Outbox mesajını yayınlandı olarak işaretler.</summary>
    Task MarkOutboxPublishedAsync(long outboxId, CancellationToken cancellationToken = default);

    /// <summary>Outbox yeniden deneme sayacını artırır.</summary>
    Task IncrementOutboxRetryAsync(long outboxId, CancellationToken cancellationToken = default);
}

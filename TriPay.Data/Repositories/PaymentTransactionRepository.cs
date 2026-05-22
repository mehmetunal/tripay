using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TriPay.Core.Options;
using TriPay.Data.Entities;
using TriPay.Data.Persistence;

namespace TriPay.Data.Repositories;

/// <summary><see cref="IPaymentTransactionRepository"/> EF Core uygulaması.</summary>
public sealed class PaymentTransactionRepository : IPaymentTransactionRepository
{
    private readonly TriPayDbContext _db;
    private readonly IOptionsMonitor<TriPayPersistenceOptions> _persistence;

    /// <summary>Repository oluşturur.</summary>
    public PaymentTransactionRepository(TriPayDbContext db, IOptionsMonitor<TriPayPersistenceOptions> persistence)
    {
        _db = db;
        _persistence = persistence;
    }

    /// <summary>Kanal koduna göre gateway kimliğini döner.</summary>
    public Task<int?> GetGatewayIdByCodeAsync(string gatewayCode, CancellationToken cancellationToken = default)
        => _db.PaymentGateways
            .Where(g => g.Code == gatewayCode && g.IsActive)
            .Select(g => (int?)g.Id)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>Demo üye işyeri kimliğini döner.</summary>
    public async Task<int> GetDefaultMerchantIdAsync(CancellationToken cancellationToken = default)
    {
        var id = await _db.Merchants
            .Where(m => m.IsActive)
            .OrderBy(m => m.Id)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (id == 0)
            throw new InvalidOperationException("Aktif üye işyeri bulunamadı. Migration seed çalıştırıldı mı?");

        return id;
    }

    /// <summary>Üye işyeri ve sipariş numarasına göre işlem getirir.</summary>
    public Task<PaymentTransaction?> GetByOrderAsync(int merchantId, string orderNumber, CancellationToken cancellationToken = default)
        => _db.Transactions
            .FirstOrDefaultAsync(t => t.MerchantId == merchantId && t.OrderNumber == orderNumber, cancellationToken);

    /// <summary>Yeni işlem ekler.</summary>
    public async Task<PaymentTransaction> AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
    {
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    /// <summary>İşlem kaydını günceller.</summary>
    public async Task UpdateAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
    {
        _db.Transactions.Update(transaction);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>İşlem logu ekler (<c>PersistTransactionLogs=false</c> ise yazılmaz).</summary>
    public async Task AddLogAsync(PaymentTransactionLog log, CancellationToken cancellationToken = default)
    {
        if (!_persistence.CurrentValue.PersistTransactionLogs)
            return;

        _db.TransactionLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Outbox mesajı ekler (<c>EnableOutbox=false</c> ise yazılmaz).</summary>
    public async Task AddOutboxAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        if (!_persistence.CurrentValue.EnableOutbox)
            return;

        _db.OutboxMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Yayınlanmamış outbox mesajlarını getirir.</summary>
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingOutboxAsync(int take, CancellationToken cancellationToken = default)
        => await _db.OutboxMessages
            .Where(m => !m.IsPublished && m.RetryCount < 10)
            .OrderBy(m => m.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    /// <summary>Outbox mesajını yayınlandı olarak işaretler.</summary>
    public async Task MarkOutboxPublishedAsync(long outboxId, CancellationToken cancellationToken = default)
    {
        var msg = await _db.OutboxMessages.FindAsync([outboxId], cancellationToken);
        if (msg == null) return;
        msg.IsPublished = true;
        msg.PublishedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Outbox yeniden deneme sayacını artırır.</summary>
    public async Task IncrementOutboxRetryAsync(long outboxId, CancellationToken cancellationToken = default)
    {
        var msg = await _db.OutboxMessages.FindAsync([outboxId], cancellationToken);
        if (msg == null) return;
        msg.RetryCount++;
        await _db.SaveChangesAsync(cancellationToken);
    }
}

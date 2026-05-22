using TriPay.Data.Entities;

namespace TriPay.Data.Repositories.Admin;

/// <summary>Outbox admin veri erişimi.</summary>
public interface IAdminOutboxRepository
{
    Task<AdminPagedResult<AdminOutboxListRow>> ListAsync(AdminOutboxQuery query, CancellationToken cancellationToken = default);
    Task<OutboxMessage?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> RequeueAsync(long id, CancellationToken cancellationToken = default);
}

public sealed class AdminOutboxQuery
{
    public bool? UnpublishedOnly { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed record AdminOutboxListRow(
    long Id,
    int TransactionId,
    string RoutingKey,
    bool IsPublished,
    int RetryCount,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    string PayloadPreview);

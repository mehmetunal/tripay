using Microsoft.EntityFrameworkCore;
using TriPay.Data.Entities;
using TriPay.Data.Persistence;

namespace TriPay.Data.Repositories.Admin;

/// <summary><see cref="IAdminOutboxRepository"/> EF Core uygulaması.</summary>
public sealed class AdminOutboxRepository : IAdminOutboxRepository
{
    private readonly TriPayDbContext _db;

    public AdminOutboxRepository(TriPayDbContext db) => _db = db;

    public async Task<AdminPagedResult<AdminOutboxListRow>> ListAsync(
        AdminOutboxQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.OutboxMessages.AsNoTracking().AsQueryable();

        if (query.UnpublishedOnly == true)
            q = q.Where(o => !o.IsPublished);

        var total = await q.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(query.PageSize, 5, 100);
        var skip = Math.Max(0, (Math.Max(1, query.Page) - 1) * pageSize);

        var items = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(o => new AdminOutboxListRow(
                o.Id,
                o.TransactionId,
                o.RoutingKey,
                o.IsPublished,
                o.RetryCount,
                o.CreatedAt,
                o.PublishedAt,
                o.Payload.Length > 120 ? o.Payload.Substring(0, 120) + "…" : o.Payload))
            .ToListAsync(cancellationToken);

        return new AdminPagedResult<AdminOutboxListRow>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = pageSize
        };
    }

    public Task<OutboxMessage?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _db.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<bool> RequeueAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _db.OutboxMessages.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (item == null)
            return false;

        item.IsPublished = false;
        item.PublishedAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

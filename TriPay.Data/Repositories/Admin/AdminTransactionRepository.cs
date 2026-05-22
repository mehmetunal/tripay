using Microsoft.EntityFrameworkCore;
using TriPay.Data.Entities;
using TriPay.Data.Persistence;

namespace TriPay.Data.Repositories.Admin;

/// <summary><see cref="IAdminTransactionRepository"/> EF Core uygulaması.</summary>
public sealed class AdminTransactionRepository : IAdminTransactionRepository
{
    private readonly TriPayDbContext _db;

    public AdminTransactionRepository(TriPayDbContext db) => _db = db;

    public async Task<AdminPagedResult<AdminTransactionListRow>> ListAsync(
        AdminTransactionQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.Transactions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.OrderNumber))
            q = q.Where(t => t.OrderNumber.Contains(query.OrderNumber));

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(t => t.Status == query.Status);

        if (query.MerchantId.HasValue)
            q = q.Where(t => t.MerchantId == query.MerchantId.Value);

        if (query.PaymentGatewayId.HasValue)
            q = q.Where(t => t.PaymentGatewayId == query.PaymentGatewayId.Value);

        if (query.FromUtc.HasValue)
            q = q.Where(t => t.CreatedAt >= query.FromUtc.Value);

        if (query.ToUtc.HasValue)
            q = q.Where(t => t.CreatedAt <= query.ToUtc.Value);

        var total = await q.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(query.PageSize, 5, 100);
        var skip = Math.Max(0, (Math.Max(1, query.Page) - 1) * pageSize);

        var items = await q
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(t => new AdminTransactionListRow(
                t.Id,
                t.OrderNumber,
                t.Merchant!.Name,
                t.PaymentGateway!.Code,
                t.Amount,
                t.Currency,
                t.Status,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AdminPagedResult<AdminTransactionListRow>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = pageSize
        };
    }

    public Task<PaymentTransaction?> GetDetailAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Transactions.AsNoTracking()
            .Include(t => t.Merchant)
            .Include(t => t.PaymentGateway)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AdminTransactionLogRow>> GetLogsAsync(int transactionId, CancellationToken cancellationToken = default) =>
        await _db.TransactionLogs.AsNoTracking()
            .Where(l => l.TransactionId == transactionId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new AdminTransactionLogRow(
                l.Id,
                l.LogType,
                l.Direction,
                l.GatewayCode,
                l.HttpStatusCode,
                l.ErrorCode,
                l.RequestPayload,
                l.ResponsePayload,
                l.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AdminLookupRow>> ListMerchantLookupsAsync(CancellationToken cancellationToken = default) =>
        await _db.Merchants.AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new AdminLookupRow(m.Id, m.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AdminLookupRow>> ListGatewayLookupsAsync(CancellationToken cancellationToken = default) =>
        await _db.PaymentGateways.AsNoTracking()
            .OrderBy(g => g.Code)
            .Select(g => new AdminLookupRow(g.Id, g.Code))
            .ToListAsync(cancellationToken);
}

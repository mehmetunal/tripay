using Microsoft.EntityFrameworkCore;
using TriPay.Data.Constants;
using TriPay.Data.Persistence;

namespace TriPay.Data.Repositories.Admin;

/// <summary><see cref="IAdminReportsRepository"/> EF Core uygulaması.</summary>
public sealed class AdminReportsRepository : IAdminReportsRepository
{
    private readonly TriPayDbContext _db;

    public AdminReportsRepository(TriPayDbContext db) => _db = db;

    public async Task<AdminReportSummaryRow> GetSummaryAsync(
        AdminReportsQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = Filter(query);
        var rows = await q
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Success = g.Count(t => t.Status == TransactionStatuses.Success),
                Failed = g.Count(t => t.Status == TransactionStatuses.Failed),
                Pending = g.Count(t => t.Status == TransactionStatuses.Pending),
                Cancelled = g.Count(t => t.Status == TransactionStatuses.Cancelled),
                SuccessAmount = g.Where(t => t.Status == TransactionStatuses.Success).Sum(t => (decimal?)t.Amount) ?? 0,
                Currency = g.Select(t => t.Currency).FirstOrDefault() ?? "TRY"
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (rows == null)
            return new AdminReportSummaryRow(0, 0, 0, 0, 0, 0, "TRY");

        return new AdminReportSummaryRow(
            rows.Total,
            rows.Success,
            rows.Failed,
            rows.Pending,
            rows.Cancelled,
            rows.SuccessAmount,
            rows.Currency);
    }

    public async Task<IReadOnlyList<AdminReportMerchantRow>> GetByMerchantAsync(
        AdminReportsQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = Filter(query);
        return await q
            .GroupBy(t => new { t.MerchantId, Name = t.Merchant!.Name })
            .Select(g => new AdminReportMerchantRow(
                g.Key.MerchantId,
                g.Key.Name,
                g.Count(),
                g.Count(t => t.Status == TransactionStatuses.Success),
                g.Where(t => t.Status == TransactionStatuses.Success).Sum(t => (decimal?)t.Amount) ?? 0,
                g.Select(t => t.Currency).FirstOrDefault() ?? "TRY"))
            .OrderByDescending(r => r.SuccessAmount)
            .ThenByDescending(r => r.TotalCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminReportGatewayRow>> GetByGatewayAsync(
        AdminReportsQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = Filter(query);
        return await q
            .GroupBy(t => new { t.PaymentGatewayId, Code = t.PaymentGateway!.Code })
            .Select(g => new AdminReportGatewayRow(
                g.Key.PaymentGatewayId,
                g.Key.Code,
                g.Count(),
                g.Count(t => t.Status == TransactionStatuses.Success),
                g.Where(t => t.Status == TransactionStatuses.Success).Sum(t => (decimal?)t.Amount) ?? 0,
                g.Select(t => t.Currency).FirstOrDefault() ?? "TRY"))
            .OrderByDescending(r => r.SuccessAmount)
            .ThenByDescending(r => r.TotalCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminReportDailyRow>> GetDailyTrendAsync(
        AdminReportsQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = Filter(query);
        return await q
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new AdminReportDailyRow(
                g.Key,
                g.Count(),
                g.Count(t => t.Status == TransactionStatuses.Success),
                g.Where(t => t.Status == TransactionStatuses.Success).Sum(t => (decimal?)t.Amount) ?? 0))
            .OrderBy(r => r.Date)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Entities.PaymentTransaction> Filter(AdminReportsQuery query)
    {
        var q = _db.Transactions.AsNoTracking()
            .Include(t => t.Merchant)
            .Include(t => t.PaymentGateway)
            .Where(t => t.CreatedAt >= query.FromUtc && t.CreatedAt <= query.ToUtc);

        if (query.MerchantId.HasValue)
            q = q.Where(t => t.MerchantId == query.MerchantId.Value);

        if (query.PaymentGatewayId.HasValue)
            q = q.Where(t => t.PaymentGatewayId == query.PaymentGatewayId.Value);

        return q;
    }
}

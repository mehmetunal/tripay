using TriPay.Admin.Application.Dtos;
using TriPay.Admin.Application.Dtos.Reports;
using TriPay.Data.Repositories.Admin;

namespace TriPay.Admin.Application.Services;

/// <summary>Ödeme raporları iş kuralları.</summary>
public sealed class AdminReportsService : IAdminReportsService
{
    private static readonly TimeSpan DefaultRange = TimeSpan.FromDays(30);

    private readonly IAdminReportsRepository _reports;
    private readonly IAdminTransactionRepository _transactions;

    public AdminReportsService(IAdminReportsRepository reports, IAdminTransactionRepository transactions)
    {
        _reports = reports;
        _transactions = transactions;
    }

    public ReportsFilterDto NormalizeFilter(ReportsFilterDto? filter)
    {
        var to = filter?.ToUtc ?? DateTime.UtcNow;
        var from = filter?.FromUtc ?? to.Subtract(DefaultRange);

        if (from > to)
            (from, to) = (to, from);

        return new ReportsFilterDto
        {
            FromUtc = from.Date,
            ToUtc = to.Date.AddDays(1).AddTicks(-1),
            MerchantId = filter?.MerchantId,
            PaymentGatewayId = filter?.PaymentGatewayId
        };
    }

    public async Task<ReportsIndexDto> GetIndexAsync(
        ReportsFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeFilter(filter);
        var query = ToQuery(normalized);

        var summaryRow = await _reports.GetSummaryAsync(query, cancellationToken);
        var summary = new ReportSummaryDto
        {
            TotalCount = summaryRow.TotalCount,
            SuccessCount = summaryRow.SuccessCount,
            FailedCount = summaryRow.FailedCount,
            PendingCount = summaryRow.PendingCount,
            CancelledCount = summaryRow.CancelledCount,
            SuccessAmount = summaryRow.SuccessAmount,
            Currency = summaryRow.Currency,
            SuccessRatePercent = summaryRow.TotalCount == 0
                ? 0
                : Math.Round(100m * summaryRow.SuccessCount / summaryRow.TotalCount, 1)
        };

        var merchants = await _reports.GetByMerchantAsync(query, cancellationToken);
        var gateways = await _reports.GetByGatewayAsync(query, cancellationToken);
        var daily = await _reports.GetDailyTrendAsync(query, cancellationToken);

        var merchantLookups = await _transactions.ListMerchantLookupsAsync(cancellationToken);
        var gatewayLookups = await _transactions.ListGatewayLookupsAsync(cancellationToken);

        return new ReportsIndexDto
        {
            Filter = normalized,
            Summary = summary,
            ByMerchant = merchants.Select(m => new ReportMerchantRowDto
            {
                MerchantId = m.MerchantId,
                MerchantName = m.MerchantName,
                TotalCount = m.TotalCount,
                SuccessCount = m.SuccessCount,
                SuccessAmount = m.SuccessAmount,
                Currency = m.Currency
            }).ToList(),
            ByGateway = gateways.Select(g => new ReportGatewayRowDto
            {
                PaymentGatewayId = g.PaymentGatewayId,
                GatewayCode = g.GatewayCode,
                TotalCount = g.TotalCount,
                SuccessCount = g.SuccessCount,
                SuccessAmount = g.SuccessAmount,
                Currency = g.Currency
            }).ToList(),
            DailyTrend = daily.Select(d => new ReportDailyRowDto
            {
                Date = d.Date,
                TotalCount = d.TotalCount,
                SuccessCount = d.SuccessCount,
                SuccessAmount = d.SuccessAmount
            }).ToList(),
            Merchants = merchantLookups.Select(m => new LookupDto { Id = m.Id, Name = m.Name }).ToList(),
            Gateways = gatewayLookups.Select(g => new LookupDto { Id = g.Id, Name = g.Name }).ToList()
        };
    }

    private static AdminReportsQuery ToQuery(ReportsFilterDto filter) => new()
    {
        FromUtc = filter.FromUtc,
        ToUtc = filter.ToUtc,
        MerchantId = filter.MerchantId,
        PaymentGatewayId = filter.PaymentGatewayId
    };
}

namespace TriPay.Data.Repositories.Admin;

/// <summary>Ödeme raporları veri erişimi.</summary>
public interface IAdminReportsRepository
{
    Task<AdminReportSummaryRow> GetSummaryAsync(AdminReportsQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminReportMerchantRow>> GetByMerchantAsync(AdminReportsQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminReportGatewayRow>> GetByGatewayAsync(AdminReportsQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminReportDailyRow>> GetDailyTrendAsync(AdminReportsQuery query, CancellationToken cancellationToken = default);
}

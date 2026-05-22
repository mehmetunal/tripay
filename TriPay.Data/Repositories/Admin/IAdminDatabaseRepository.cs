namespace TriPay.Data.Repositories.Admin;

/// <summary>Veritabanı durum ve özet sorguları (admin).</summary>
public interface IAdminDatabaseRepository
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
    Task<long?> GetLatestMigrationVersionAsync(CancellationToken cancellationToken = default);
    Task<AdminDashboardStatsRow> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}

public sealed record AdminDashboardStatsRow(
    int TransactionCount,
    int SuccessCount,
    int FailedCount,
    int PendingOutboxCount,
    int MerchantCount,
    int GatewayCount);

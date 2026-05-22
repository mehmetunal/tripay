using Microsoft.EntityFrameworkCore;
using TriPay.Data.Constants;
using TriPay.Data.Persistence;

namespace TriPay.Data.Repositories.Admin;

/// <summary><see cref="IAdminDatabaseRepository"/> EF Core uygulaması.</summary>
public sealed class AdminDatabaseRepository : IAdminDatabaseRepository
{
    private readonly TriPayDbContext _db;

    public AdminDatabaseRepository(TriPayDbContext db) => _db = db;

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) =>
        _db.Database.CanConnectAsync(cancellationToken);

    public async Task<long?> GetLatestMigrationVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var cmd = _db.Database.GetDbConnection().CreateCommand();
            if (cmd.Connection!.State != System.Data.ConnectionState.Open)
                await cmd.Connection.OpenAsync(cancellationToken);

            cmd.CommandText = "SELECT MAX([Version]) FROM [VersionInfo]";
            var scalar = await cmd.ExecuteScalarAsync(cancellationToken);
            if (scalar is null or DBNull)
                return null;

            return Convert.ToInt64(scalar);
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminDashboardStatsRow> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var txn = await _db.Transactions.AsNoTracking().Select(t => t.Status).ToListAsync(cancellationToken);

        return new AdminDashboardStatsRow(
            txn.Count,
            txn.Count(s => s == TransactionStatuses.Success),
            txn.Count(s => s == TransactionStatuses.Failed),
            await _db.OutboxMessages.CountAsync(o => !o.IsPublished, cancellationToken),
            await _db.Merchants.CountAsync(cancellationToken),
            await _db.PaymentGateways.CountAsync(cancellationToken));
    }
}

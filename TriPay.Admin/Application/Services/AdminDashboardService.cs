using TriPay.Admin.Application.Dtos.Dashboard;
using TriPay.Core.Redis;
using TriPay.Data.Repositories.Admin;

namespace TriPay.Admin.Application.Services;

/// <summary>Dashboard özet verileri.</summary>
public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDatabaseRepository _database;
    private readonly ITriPayRedisCache _redis;

    public AdminDashboardService(IAdminDatabaseRepository database, ITriPayRedisCache redis)
    {
        _database = database;
        _redis = redis;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _database.GetDashboardStatsAsync(cancellationToken);

        return new DashboardStatsDto
        {
            TransactionCount = stats.TransactionCount,
            SuccessCount = stats.SuccessCount,
            FailedCount = stats.FailedCount,
            PendingOutboxCount = stats.PendingOutboxCount,
            MerchantCount = stats.MerchantCount,
            GatewayCount = stats.GatewayCount,
            DatabaseOk = await _database.CanConnectAsync(cancellationToken),
            RedisOk = await _redis.PingAsync(cancellationToken)
        };
    }
}

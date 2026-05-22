namespace TriPay.Admin.Application.Dtos.Dashboard;

public sealed class DashboardStatsDto
{
    public int TransactionCount { get; init; }
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public int PendingOutboxCount { get; init; }
    public int MerchantCount { get; init; }
    public int GatewayCount { get; init; }
    public bool DatabaseOk { get; init; }
    public bool RedisOk { get; init; }
}

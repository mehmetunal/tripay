using TriPay.Admin.Application.Dtos.Dashboard;

namespace TriPay.Admin.Application.Services;

public interface IAdminDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}

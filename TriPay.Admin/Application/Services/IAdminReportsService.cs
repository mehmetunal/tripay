using TriPay.Admin.Application.Dtos.Reports;

namespace TriPay.Admin.Application.Services;

public interface IAdminReportsService
{
    Task<ReportsIndexDto> GetIndexAsync(ReportsFilterDto filter, CancellationToken cancellationToken = default);
    ReportsFilterDto NormalizeFilter(ReportsFilterDto? filter);
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriPay.Admin.Application.Mappings;
using TriPay.Admin.Application.Services;
using TriPay.Admin.Authorization;
using TriPay.Admin.Infrastructure;
using TriPay.Data.Identity;

namespace TriPay.Admin.Controllers;

/// <summary>Yönetim paneli ana sayfa.</summary>
[Authorize(Policy = AdminPolicies.DashboardView)]
public sealed class DashboardController : Controller
{
    private readonly IAdminDashboardService _dashboard;

    public DashboardController(IAdminDashboardService dashboard) => _dashboard = dashboard;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var dto = await _dashboard.GetStatsAsync(cancellationToken);
        var model = AdminDtoMapper.ToViewModel(dto);
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_IndexContent", model) : View(model);
    }
}

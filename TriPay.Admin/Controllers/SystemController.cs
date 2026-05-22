using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriPay.Admin.Application.Mappings;
using TriPay.Admin.Application.Services;
using TriPay.Admin.Authorization;
using TriPay.Admin.Infrastructure;
using TriPay.Data.Identity;

namespace TriPay.Admin.Controllers;

/// <summary>Sistem durumu ve önbellek işlemleri.</summary>
[Authorize(Policy = AdminPolicies.SystemView)]
public sealed class SystemController : Controller
{
    private readonly IAdminSystemService _system;

    public SystemController(IAdminSystemService system) => _system = system;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var dto = await _system.GetStatusAsync(cancellationToken);
        var model = AdminDtoMapper.ToViewModel(dto);
        ViewData["AdminModule"] = "system.index";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_IndexContent", model) : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AdminPolicies.SystemManage)]
    public async Task<IActionResult> ClearGatewayCache(CancellationToken cancellationToken)
    {
        await _system.ClearGatewayCachesAsync(cancellationToken);

        if (AdminMvcAjax.IsAjaxRequest(Request))
            return AdminMvcAjax.JsonSuccess("Gateway metadata önbelleği temizlendi.", Url.Action(nameof(Index))!);

        TempData["Success"] = "Gateway metadata önbelleği temizlendi.";
        return RedirectToAction(nameof(Index));
    }
}

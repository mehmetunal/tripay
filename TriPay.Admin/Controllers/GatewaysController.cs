using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriPay.Admin.Application.Mappings;
using TriPay.Admin.Application.Services;
using TriPay.Admin.Authorization;
using TriPay.Admin.Infrastructure;
using TriPay.Admin.Models.Gateways;
using TriPay.Data.Identity;

namespace TriPay.Admin.Controllers;

/// <summary>Ödeme kanalları, teknik ayarlar ve hata sözlüğü.</summary>
[Authorize(Policy = AdminPolicies.GatewaysView)]
public sealed class GatewaysController : Controller
{
    private readonly IAdminGatewayService _gateways;

    public GatewaysController(IAdminGatewayService gateways) => _gateways = gateways;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await _gateways.ListGatewaysAsync(cancellationToken);
        ViewData["AdminModule"] = "gateways.index";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_IndexContent", list) : View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AdminPolicies.GatewaysManage)]
    public async Task<IActionResult> ClearAllCache(CancellationToken cancellationToken)
    {
        await _gateways.InvalidateAllCachesAsync(cancellationToken);
        return SuccessRedirect("Tüm gateway önbellekleri temizlendi.", nameof(Index));
    }

    public async Task<IActionResult> Settings(int gatewayId, CancellationToken cancellationToken)
    {
        var gateway = await _gateways.GetGatewayContextAsync(gatewayId, cancellationToken);
        if (gateway == null)
            return NotFound();

        var settings = await _gateways.ListSettingsAsync(gatewayId, cancellationToken);
        ViewBag.Gateway = gateway;
        ViewData["AdminModule"] = "gateways.settings";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_SettingsContent", settings) : View(settings);
    }

    public async Task<IActionResult> CreateSetting(int gatewayId, CancellationToken cancellationToken)
    {
        try
        {
            var model = AdminDtoMapper.ToSettingViewModel(await _gateways.CreateSettingFormAsync(gatewayId, cancellationToken));
            ViewData["AdminModule"] = "gateways.settingForm";
            return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_SettingFormContent", model) : View(model);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AdminPolicies.GatewaysManage)]
    public async Task<IActionResult> CreateSetting(GatewaySettingEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        await _gateways.CreateSettingAsync(AdminDtoMapper.ToUpsertDto(model), cancellationToken);
        return SuccessRedirect("Ayar eklendi; önbellek temizlendi.", nameof(Settings), new { gatewayId = model.PaymentGatewayId });
    }

    public async Task<IActionResult> EditSetting(int id, CancellationToken cancellationToken)
    {
        var dto = await _gateways.GetSettingForEditAsync(id, cancellationToken);
        if (dto == null)
            return NotFound();

        var model = AdminDtoMapper.ToSettingViewModel(dto);
        ViewData["AdminModule"] = "gateways.settingForm";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_SettingFormContent", model) : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AdminPolicies.GatewaysManage)]
    public async Task<IActionResult> EditSetting(GatewaySettingEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        if (!await _gateways.UpdateSettingAsync(AdminDtoMapper.ToUpsertDto(model), cancellationToken))
            return NotFound();

        return SuccessRedirect("Ayar güncellendi; önbellek temizlendi.", nameof(Settings), new { gatewayId = model.PaymentGatewayId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AdminPolicies.GatewaysManage)]
    public async Task<IActionResult> DeleteSetting(int id, string gatewayCode, int gatewayId, CancellationToken cancellationToken)
    {
        await _gateways.DeleteSettingAsync(id, cancellationToken);
        return SuccessRedirect("Ayar silindi.", nameof(Settings), new { gatewayId });
    }

    public async Task<IActionResult> Errors(int gatewayId, CancellationToken cancellationToken)
    {
        var gateway = await _gateways.GetGatewayContextAsync(gatewayId, cancellationToken);
        if (gateway == null)
            return NotFound();

        var errors = await _gateways.ListErrorsAsync(gatewayId, cancellationToken);
        ViewBag.Gateway = gateway;
        ViewData["AdminModule"] = "gateways.errors";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_ErrorsContent", errors) : View(errors);
    }

    public async Task<IActionResult> CreateError(int gatewayId, CancellationToken cancellationToken)
    {
        try
        {
            var model = AdminDtoMapper.ToErrorViewModel(await _gateways.CreateErrorFormAsync(gatewayId, cancellationToken));
            ViewData["AdminModule"] = "gateways.errorForm";
            return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_ErrorFormContent", model) : View(model);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AdminPolicies.GatewaysManage)]
    public async Task<IActionResult> CreateError(GatewayErrorEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        await _gateways.CreateErrorAsync(AdminDtoMapper.ToUpsertDto(model), cancellationToken);
        return SuccessRedirect("Hata eşlemesi eklendi.", nameof(Errors), new { gatewayId = model.PaymentGatewayId });
    }

    public async Task<IActionResult> EditError(int id, CancellationToken cancellationToken)
    {
        var dto = await _gateways.GetErrorForEditAsync(id, cancellationToken);
        if (dto == null)
            return NotFound();

        var model = AdminDtoMapper.ToErrorViewModel(dto);
        ViewData["AdminModule"] = "gateways.errorForm";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_ErrorFormContent", model) : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AdminPolicies.GatewaysManage)]
    public async Task<IActionResult> EditError(GatewayErrorEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        if (!await _gateways.UpdateErrorAsync(AdminDtoMapper.ToUpsertDto(model), cancellationToken))
            return NotFound();

        return SuccessRedirect("Hata eşlemesi güncellendi.", nameof(Errors), new { gatewayId = model.PaymentGatewayId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AdminPolicies.GatewaysManage)]
    public async Task<IActionResult> DeleteError(int id, string gatewayCode, int gatewayId, CancellationToken cancellationToken)
    {
        await _gateways.DeleteErrorAsync(id, cancellationToken);
        return SuccessRedirect("Hata eşlemesi silindi.", nameof(Errors), new { gatewayId });
    }

    private IActionResult SuccessRedirect(string message, string action, object? routeValues = null)
    {
        var url = Url.Action(action, routeValues)!;
        if (AdminMvcAjax.IsAjaxRequest(Request))
            return AdminMvcAjax.JsonSuccess(message, url);
        TempData["Success"] = message;
        return RedirectToAction(action, routeValues);
    }
}

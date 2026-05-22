using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriPay.Admin.Application.Mappings;
using TriPay.Admin.Application.Services;
using TriPay.Admin.Authorization;
using TriPay.Admin.Infrastructure;
using TriPay.Admin.Models.Merchants;
using TriPay.Data.Identity;

namespace TriPay.Admin.Controllers;

/// <summary>Üye işyeri yönetimi.</summary>
[Authorize(Policy = AdminPolicies.MerchantsView)]
public sealed class MerchantsController : Controller
{
    private readonly IAdminMerchantService _merchants;

    public MerchantsController(IAdminMerchantService merchants) => _merchants = merchants;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await _merchants.ListAsync(cancellationToken);
        var model = list.Select(AdminDtoMapper.ToListItem).ToList();
        ViewData["AdminModule"] = "merchants.index";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_IndexContent", model) : View(model);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var dto = await _merchants.GetForEditAsync(id, cancellationToken);
        if (dto == null)
            return NotFound();

        ViewData["AdminModule"] = "merchants.edit";
        var model = AdminDtoMapper.ToEditViewModel(dto);
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_EditContent", model) : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AdminPolicies.MerchantsManage)]
    public async Task<IActionResult> Edit(MerchantEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        var updated = await _merchants.UpdateAsync(AdminDtoMapper.ToUpdateDto(model), cancellationToken);
        if (!updated)
            return NotFound();

        if (AdminMvcAjax.IsAjaxRequest(Request))
            return AdminMvcAjax.JsonSuccess("Üye işyeri güncellendi.", Url.Action(nameof(Index))!);

        TempData["Success"] = "Üye işyeri güncellendi.";
        return RedirectToAction(nameof(Index));
    }
}

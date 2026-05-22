using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriPay.Admin.Authorization;
using TriPay.Admin.Infrastructure;
using TriPay.Admin.Models.Roles;
using TriPay.Data.Identity;

namespace TriPay.Admin.Controllers;

/// <summary>Rol ve yetki yönetimi (veritabanı — AspNetRoleClaims).</summary>
[Authorize(Policy = AdminPolicies.RolesManage)]
public sealed class RolesController : Controller
{
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public RolesController(RoleManager<IdentityRole<int>> roleManager) => _roleManager = roleManager;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync(cancellationToken);
        var list = new List<RoleListItem>();

        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var perms = claims.Where(c => c.Type == AdminPermissions.ClaimType).Select(c => c.Value).ToList();
            list.Add(new RoleListItem
            {
                Id = role.Id,
                Name = role.Name ?? "",
                DisplayName = AdminRoles.GetDisplayName(role.Name ?? ""),
                IsAdminRole = AdminRoles.IsAdminRole(role.Name),
                PermissionCount = AdminRoles.IsAdminRole(role.Name)
                    ? AdminPermissions.AllCodes.Length
                    : perms.Count
            });
        }

        ViewData["AdminModule"] = "roles.index";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_IndexContent", list) : View(list);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            return NotFound();

        if (AdminRoles.IsAdminRole(role.Name))
        {
            TempData["Error"] = "Admin rolünün izinleri kod tarafında tam yetkidir; düzenlenemez.";
            return RedirectToAction(nameof(Index));
        }

        var claims = await _roleManager.GetClaimsAsync(role);
        var selected = claims
            .Where(c => c.Type == AdminPermissions.ClaimType)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var model = new RoleEditViewModel
        {
            Id = role.Id,
            RoleName = role.Name ?? "",
            DisplayName = AdminRoles.GetDisplayName(role.Name ?? ""),
            Permissions = AdminPermissions.Definitions
                .Select(d => new RolePermissionItem
                {
                    Code = d.Code,
                    Label = d.Label,
                    Description = d.Description,
                    IsSelected = selected.Contains(d.Code)
                })
                .ToList()
        };

        ViewData["AdminModule"] = "roles.edit";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_EditContent", model) : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleEditViewModel model, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(model.Id.ToString());
        if (role == null)
            return NotFound();

        if (AdminRoles.IsAdminRole(role.Name))
            return RedirectToAction(nameof(Index));

        var selected = model.Permissions?.Where(p => p.IsSelected).Select(p => p.Code).ToList()
                       ?? [];

        if (!ModelState.IsValid)
        {
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            ViewData["AdminModule"] = "roles.edit";
            return View(model);
        }

        await AdminPermissionSeeder.SyncRoleClaimsAsync(_roleManager, role.Name!, selected, cancellationToken: cancellationToken);

        if (AdminMvcAjax.IsAjaxRequest(Request))
            return AdminMvcAjax.JsonSuccess("Rol yetkileri güncellendi.", Url.Action(nameof(Index))!);

        TempData["Success"] = "Rol yetkileri güncellendi.";
        return RedirectToAction(nameof(Index));
    }
}

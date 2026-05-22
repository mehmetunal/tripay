using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TriPay.Admin.Authorization;
using TriPay.Admin.Infrastructure;
using TriPay.Admin.Models.Users;
using TriPay.Data.Entities;
using TriPay.Data.Identity;

namespace TriPay.Admin.Controllers;

/// <summary>Panel kullanıcı yönetimi.</summary>
[Authorize(Policy = AdminPolicies.UsersManage)]
public sealed class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync(cancellationToken);
        var list = new List<UserListItem>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            list.Add(new UserListItem
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? "-",
                DisplayName = u.DisplayName,
                Roles = roles.ToList(),
                EmailConfirmed = u.EmailConfirmed,
                LockoutEnabled = u.LockoutEnabled,
                IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow
            });
        }

        ViewData["AdminModule"] = "users.index";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_IndexContent", list) : View(list);
    }

    public IActionResult Create()
    {
        ViewData["AdminModule"] = "users.create";
        ViewBag.Roles = BuildRoleSelectList(AdminRole.User.ToRoleName());
        var model = new CreateUserViewModel();
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_CreateContent", model) : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = BuildRoleSelectList(model.RoleName);
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            DisplayName = model.DisplayName?.Trim()
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            ViewBag.Roles = BuildRoleSelectList(model.RoleName);
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.RoleName);

        if (AdminMvcAjax.IsAjaxRequest(Request))
            return AdminMvcAjax.JsonSuccess("Kullanıcı oluşturuldu.", Url.Action(nameof(Index))!);

        TempData["Success"] = "Kullanıcı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ResetPassword(int id, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound();

        ViewData["AdminModule"] = "users.resetPassword";
        var model = new ResetPasswordViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? "-"
        };
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_ResetPasswordContent", model) : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId.ToString());
        if (user == null)
            return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        if (AdminMvcAjax.IsAjaxRequest(Request))
            return AdminMvcAjax.JsonSuccess("Şifre güncellendi.", Url.Action(nameof(Index))!);

        TempData["Success"] = "Şifre güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(int id, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound();

        var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
        var result = isLocked
            ? await _userManager.SetLockoutEndDateAsync(user, null)
            : await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(50));

        if (!result.Succeeded)
        {
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return new JsonResult(new { success = false, message = "Kilit durumu güncellenemedi." }) { StatusCode = 400 };
            TempData["Error"] = "Kilit durumu güncellenemedi.";
            return RedirectToAction(nameof(Index));
        }

        var msg = isLocked ? "Kullanıcı kilidi kaldırıldı." : "Kullanıcı kilitlendi.";
        if (AdminMvcAjax.IsAjaxRequest(Request))
            return AdminMvcAjax.JsonSuccess(msg, Url.Action(nameof(Index))!);

        TempData["Success"] = msg;
        return RedirectToAction(nameof(Index));
    }

    private static SelectList BuildRoleSelectList(string? selected) =>
        new(AdminRoles.AllRoles.Select(r => new { Value = r.ToRoleName(), Text = r.GetDescription() }), "Value", "Text", selected);
}

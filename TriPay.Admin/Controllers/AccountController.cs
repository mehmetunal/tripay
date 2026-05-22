using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TriPay.Admin.Infrastructure;
using TriPay.Admin.Models.Account;
using TriPay.Data.Entities;
using TriPay.Data.Identity;

namespace TriPay.Admin.Controllers;

/// <summary>Yönetim paneli giriş / çıkış.</summary>
[AllowAnonymous]
public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["AdminModule"] = "account.login";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
            if (AdminMvcAjax.IsAjaxRequest(Request))
                return AdminMvcAjax.JsonValidationErrors(ModelState);
            return View(model);
        }

        var redirect = !string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
            ? model.ReturnUrl
            : Url.Action("Index", "Dashboard")!;

        if (AdminMvcAjax.IsAjaxRequest(Request))
            return AdminMvcAjax.JsonSuccess("Giriş başarılı.", redirect);

        return Redirect(redirect);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        var redirect = Url.Action(nameof(Login), "Account")!;

        if (AdminMvcAjax.IsAjaxRequest(Request))
            return AdminMvcAjax.JsonSuccess("Çıkış yapıldı.", redirect);

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}

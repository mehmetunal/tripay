using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Controllers;

public sealed class LegalController : Controller
{
    public IActionResult Kvkk() => View();
    public IActionResult Privacy() => View();
    public IActionResult Cookies() => View();
}

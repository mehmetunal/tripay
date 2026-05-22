using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Controllers;

public sealed class SecurityController : Controller
{
    public IActionResult Index() => View();
}

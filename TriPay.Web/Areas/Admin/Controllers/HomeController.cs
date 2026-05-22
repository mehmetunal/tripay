using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class HomeController : Controller
{
    [HttpGet("/admin")]
    public IActionResult Index() => View();
}

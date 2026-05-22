using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Areas.Docs.Controllers;

[Area("Docs")]
public sealed class HomeController : Controller
{
    [HttpGet("/docs")]
    public IActionResult Index() => View();
}

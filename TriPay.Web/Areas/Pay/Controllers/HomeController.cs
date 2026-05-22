using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Areas.Pay.Controllers;

[Area("Pay")]
public sealed class HomeController : Controller
{
    [HttpGet("/pay")]
    public IActionResult Index() => View();

    public IActionResult HowItWorks() => View();
}

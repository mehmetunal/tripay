using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();

    public IActionResult Error() => View();
}

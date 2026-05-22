using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Controllers;

public sealed class CozumlerController : Controller
{
    public IActionResult Index() => View();
}

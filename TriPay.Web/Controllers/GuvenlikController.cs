using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Controllers;

public sealed class GuvenlikController : Controller
{
    public IActionResult Index() => View();
}

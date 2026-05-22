using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Controllers;

public sealed class UrunController : Controller
{
    public IActionResult Index() => View();
}

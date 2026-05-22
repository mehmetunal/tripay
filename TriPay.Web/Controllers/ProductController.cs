using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Controllers;

public sealed class ProductController : Controller
{
    public IActionResult Index() => View();
}

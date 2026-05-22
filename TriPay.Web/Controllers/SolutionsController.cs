using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Controllers;

public sealed class SolutionsController : Controller
{
    public IActionResult Index() => View();
}

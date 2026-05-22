using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Controllers;

public sealed class CorporateController : Controller
{
    public IActionResult About() => View();
}

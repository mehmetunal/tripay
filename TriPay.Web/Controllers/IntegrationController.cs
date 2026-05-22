using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Controllers;

public sealed class IntegrationController : Controller
{
    public IActionResult Index() => View();
}

using Microsoft.AspNetCore.Mvc;

namespace TriPay.Web.Areas.Docs.Controllers;

[Area("Docs")]
[Route("docs/Guide/[action]")]
public sealed class GuideController : Controller
{
    public IActionResult Index() => View();
    public IActionResult GettingStarted() => View();
    public IActionResult IntegrationModes() => View();
    public IActionResult Setup() => View();
    public IActionResult PaymentFlow() => View();
    public IActionResult Security() => View();
    public IActionResult Webhook() => View();
    public IActionResult Faq() => View();
}

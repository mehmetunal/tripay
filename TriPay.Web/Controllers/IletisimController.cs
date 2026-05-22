using Microsoft.AspNetCore.Mvc;
using TriPay.Web.Models;
namespace TriPay.Web.Controllers;

public sealed class IletisimController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new ContactFormModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ContactFormModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        TempData["ContactSuccess"] = true;
        return RedirectToAction(nameof(ThankYou));
    }

    public IActionResult ThankYou()
    {
        if (TempData["ContactSuccess"] is not true)
            return RedirectToAction(nameof(Index));
        return View();
    }
}

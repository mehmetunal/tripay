using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TriPay.Demo.Models;

namespace TriPay.Demo.Controllers;

/// <summary>
/// Demo site ana sayfası ve yardımcı sayfalar. Ödeme akışı <see cref="CheckoutController"/> üzerindedir.
/// </summary>
public class HomeController : Controller
{
    /// <summary>Ödeme sayfasına yönlendirir (referans akış: Checkout).</summary>
    public IActionResult Index()
    {
        return RedirectToAction(nameof(CheckoutController.Index), "Checkout");
    }

    /// <summary>Gizlilik politikası sayfasını gösterir.</summary>
    public IActionResult Privacy() => View();

    /// <summary>Genel hata sayfasını istek kimliği ile birlikte gösterir.</summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

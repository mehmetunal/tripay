using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriPay.Services;
using TriPay.Services.Checkout;
using TriPay.Services.Providers.VakifPays.Models;

namespace TriPay.Controllers;

/// <summary>
/// Referans ödeme controller'ı: başlatma, 3D callback ve taksit sorgusu (kılavuz §18).
/// Tutar doğrulaması MSSQL Transactions tablosundan yapılır.
/// </summary>
public class CheckoutController : Controller
{
    private readonly IPaymentCheckoutService _checkout;

    /// <summary>Checkout servisini enjekte eder.</summary>
    public CheckoutController(IPaymentCheckoutService checkout) => _checkout = checkout;

    /// <summary>Demo ödeme formunu gösterir.</summary>
    public IActionResult Index()
    {
        var model = new PaymentRequest
        {
            Amount = 6000.00m,
            Currency = "TRY",
            InstallmentCount = 1,
            CustomerName = "Mehmet Unal",
            CustomerEmail = "unal.m1991@gmail.com",
            CustomerPhone = "05555555555",
            CustomerIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            ReturnUrl = $"{Request.Scheme}://{Request.Host}/Checkout/Callback",
            BillToAddressLine = "Merkez Mah. Ataturk Cad. No:1",
            BillToCity = "Istanbul",
            BillToCountry = "Turkey",
            BillToPostalCode = "34000",
            BillToPhone = "05555555555",
            ShipToAddressLine = "Merkez Mah. Ataturk Cad. No:1",
            ShipToCity = "Istanbul",
            ShipToCountry = "Turkey",
            ShipToPostalCode = "34000",
            ShipToPhone = "05555555555"
        };

        return View(model);
    }

    /// <summary>Formdan gelen ödeme isteğini başlatır; 3D HTML veya sonuç sayfası döner.</summary>
    [HttpPost]
    public async Task<IActionResult> Pay(PaymentRequest model, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _checkout.PayAsync(model, PaymentGatewayNames.Default, cancellationToken);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Ödeme başlatılamadı.");
                return View("Index", model);
            }

            var payment = result.Data ?? throw new InvalidOperationException("Ödeme yanıtı boş.");

            if (!string.IsNullOrWhiteSpace(payment.RedirectHtml))
                return Content(payment.RedirectHtml, "text/html", System.Text.Encoding.UTF8);

            ViewBag.Status = payment.Success ? "Success" : "Declined";
            ViewBag.Message = payment.Message;
            ViewBag.Amount = model.Amount.ToString("N2");
            ViewBag.OrderNumber = model.OrderNumber;
            ViewBag.QueryCode = "-";
            ViewBag.CallbackFields = "İşlem provider tarafından 3D yönlendirme olmadan sonuçlandırıldı.";
            return View("Result");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("Index", model);
        }
    }

    /// <summary>Bankadan dönen 3D callback formunu işler ve sonucu doğrular.</summary>
    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Callback(IFormCollection form, CancellationToken cancellationToken)
    {
        var raw = form.Keys.ToDictionary(k => k, k => form[k].ToString());
        var outcome = await _checkout.ProcessCallbackAsync(raw, PaymentGatewayNames.Default, cancellationToken);

        ViewBag.Status = outcome.Success ? "Success" : "Declined";
        ViewBag.Message = outcome.Message;
        ViewBag.Amount = outcome.AmountText;
        ViewBag.OrderNumber = outcome.OrderNumber;
        ViewBag.TransactionId = outcome.TransactionId;
        ViewBag.ResponseCode = outcome.ResponseCode;
        ViewBag.ResponseMsg = outcome.CallbackMessage;
        ViewBag.ErrorMsg = Uri.UnescapeDataString(outcome.ErrorMessage);
        ViewBag.QueryCode = outcome.QueryResponseCode;
        ViewBag.CallbackFields = outcome.CallbackFieldsDisplay;

        return View("Result");
    }

    /// <summary>Kart numarası ve tutara göre taksit seçeneklerini JSON olarak döner.</summary>
    [HttpGet]
    public async Task<IActionResult> Installments(string cardNumber, decimal amount = 0, CancellationToken cancellationToken = default)
    {
        var result = await _checkout.GetInstallmentsAsync(cardNumber, amount, PaymentGatewayNames.Default, cancellationToken);

        return Json(new
        {
            success = result.IsSuccess,
            installments = result.Data?.Installments ?? new List<TriPay.Services.Models.InstallmentOptionDto>()
        });
    }
}

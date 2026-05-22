using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriPay.Models;
using TriPay.Services.PaymentGateways.Interfaces;
using TriPay.Services.PaymentGateways.Models;
using TriPay.Services.PaymentGateways.Providers;

namespace TriPay.Controllers;

public class HomeController : Controller
{
    private static readonly ConcurrentDictionary<string, PendingPayment> PendingPayments = new();
    private readonly IPaymentGatewayService _paymentGatewayService;

    public HomeController(IPaymentGatewayService paymentGatewayService)
    {
        _paymentGatewayService = paymentGatewayService;
    }

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
            ReturnUrl = $"{Request.Scheme}://{Request.Host}/Home/Callback",
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

    [HttpPost]
    public async Task<IActionResult> Pay(PaymentRequest model)
    {
        try
        {
            PendingPayments[model.OrderNumber] = new PendingPayment
            {
                Amount = model.Amount,
                Currency = model.Currency,
                CreatedAtUtc = DateTime.UtcNow
            };

            var result = await _paymentGatewayService.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
            {
                Payment = model
            });
            var payment = result.Data ?? throw new InvalidOperationException(result.ErrorMessage ?? "Ödeme başlatılamadı.");

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

    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Callback(IFormCollection form)
    {
        var raw = form.Keys.ToDictionary(k => k, k => form[k].ToString());
        var callbackResult = await _paymentGatewayService.ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto { RawData = raw });
        var callback = callbackResult.Data ?? new PaymentGatewayCallbackResponseDto { ErrorMessage = callbackResult.ErrorMessage ?? string.Empty };
        var queryResult = await _paymentGatewayService.GetPaymentStatusAsync(callback.OrderNumber);
        var query = queryResult.Data ?? new PaymentGatewayStatusResponseDto { ResponseCode = string.Empty };

        var hasPending = PendingPayments.TryGetValue(callback.OrderNumber, out var pending);
        var amountText = raw.GetValueOrDefault("amount") ?? string.Empty;
        var callbackAmountOk = decimal.TryParse(amountText.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var cbAmount)
                               && pending != null
                               && Math.Abs(cbAmount - pending.Amount) < 0.01m;

        var success = hasPending && callbackAmountOk && callback.Success && query.Success && query.ResponseCode == "00";
        if (success) PendingPayments.TryRemove(callback.OrderNumber, out _);

        ViewBag.Status = success ? "Success" : "Declined";
        ViewBag.Message = success ? "Ödeme doğrulandı." : $"Callback/Query doğrulaması başarısız. CallbackCode={callback.ResponseCode}, QueryCode={query.ResponseCode}";
        ViewBag.Amount = amountText;
        ViewBag.OrderNumber = callback.OrderNumber;
        ViewBag.TransactionId = callback.TransactionId;
        ViewBag.ResponseCode = callback.ResponseCode;
        ViewBag.ResponseMsg = callback.Message;
        ViewBag.ErrorMsg = Uri.UnescapeDataString(callback.ErrorMessage);
        ViewBag.QueryCode = query.ResponseCode;
        ViewBag.CallbackFields = string.Join(Environment.NewLine, raw.Select(x => $"{x.Key}: {x.Value}"));

        return View("Result");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Installments(string cardNumber, decimal amount = 0)
    {
        var result = await _paymentGatewayService.GetInstallmentInfoAsync(new PaymentGatewayInstallmentRequestDto
        {
            CardNumber = cardNumber,
            Amount = amount,
            TestPlatform = true
        });
        var data = result.Data;

        return Json(new { success = result.IsSuccess, installments = data?.Installments ?? new List<InstallmentOptionDto>() });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class PendingPayment
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime CreatedAtUtc { get; set; }
}

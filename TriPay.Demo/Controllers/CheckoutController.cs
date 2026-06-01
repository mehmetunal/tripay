using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TriPay.Demo.Models;
using TriPay.Demo.Services;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services;
using TriPay.Services.Diagnostics;
using TriPay.Services.Providers.VakifPays.Models;

namespace TriPay.Demo.Controllers;

/// <summary>
/// Framework modu referans controller: <see cref="TriPay.Services.Interfaces.IPaymentGatewayService"/> +
/// üye işyeri sipariş deposu (demo: bellek içi).
/// </summary>
public class CheckoutController : Controller
{
    private readonly FrameworkDemoPaymentService _payments;
    private readonly CheckoutGatewayInfoService _gatewayInfo;
    private readonly DemoPaymentDiagnosticStore _diagnosticStore;
    private readonly IOptions<TriPayOptions> _triPayOptions;

    public CheckoutController(
        FrameworkDemoPaymentService payments,
        CheckoutGatewayInfoService gatewayInfo,
        DemoPaymentDiagnosticStore diagnosticStore,
        IOptions<TriPayOptions> triPayOptions)
    {
        _payments = payments;
        _gatewayInfo = gatewayInfo;
        _diagnosticStore = diagnosticStore;
        _triPayOptions = triPayOptions;
    }

    private string ActiveGatewayCode =>
        string.IsNullOrWhiteSpace(_triPayOptions.Value.DefaultGateway)
            ? PaymentGatewayNames.VakifPays
            : _triPayOptions.Value.DefaultGateway;

    private void SetGatewayViewData()
    {
        ViewData["Gateway"] = _gatewayInfo.GetSnapshot();
    }

    public IActionResult Index()
    {
        SetGatewayViewData();
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

    [HttpPost]
    public async Task<IActionResult> Pay(PaymentRequest model, CancellationToken cancellationToken)
    {
        SetGatewayViewData();
        try
        {
            var gateway = ActiveGatewayCode;
            if (string.IsNullOrWhiteSpace(model.OrderNumber))
                model.OrderNumber = Guid.NewGuid().ToString("N");

            PaymentDiagnosticContext.CurrentOrderNumber = model.OrderNumber;
            _diagnosticStore.ClearOrder(model.OrderNumber);

            PaymentDiagnostic.LogCheckoutPayRequest(gateway, new Dictionary<string, string?>
            {
                ["OrderNumber"] = model.OrderNumber,
                ["Amount"] = model.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["Currency"] = model.Currency,
                ["InstallmentCount"] = model.InstallmentCount.ToString(),
                ["CardNumber"] = model.CardNumber,
                ["ExpiryMonth"] = model.ExpiryMonth,
                ["ExpiryYear"] = model.ExpiryYear,
                ["Cvv"] = model.Cvv,
                ["CardOwner"] = model.CardOwner,
                ["CustomerName"] = model.CustomerName,
                ["CustomerEmail"] = model.CustomerEmail,
                ["CustomerPhone"] = model.CustomerPhone,
                ["CustomerIp"] = model.CustomerIp,
                ["ReturnUrl"] = model.ReturnUrl,
                ["Use3D"] = model.Use3D.ToString()
            });
            var result = await _payments.PayAsync(model, gateway, cancellationToken);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Ödeme başlatılamadı.");
                return View("Index", model);
            }

            var payment = result.Data ?? throw new InvalidOperationException("Ödeme yanıtı boş.");

            if (!string.IsNullOrWhiteSpace(payment.RedirectHtml))
            {
                PaymentDiagnostic.LogHtmlResponse(gateway, payment.RedirectHtml);
                // 3D bankaya yönlendirme HTML'ini doğrudan döndürmek callback dönüşünü en stabil hale getirir.
                return Content(payment.RedirectHtml, "text/html", System.Text.Encoding.UTF8);
            }

            ViewBag.PaymentEvents = _diagnosticStore.GetForOrder(model.OrderNumber);
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
    public async Task<IActionResult> Callback(IFormCollection form, CancellationToken cancellationToken)
    {
        var raw = form.Keys.ToDictionary(k => k, k => form[k].ToString());
        return await HandleCallbackAsync(raw, cancellationToken);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(CancellationToken cancellationToken)
    {
        var raw = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
        return await HandleCallbackAsync(raw, cancellationToken);
    }

    private async Task<IActionResult> HandleCallbackAsync(Dictionary<string, string> raw, CancellationToken cancellationToken)
    {
        SetGatewayViewData();
        var orderNumber = ResolveOrderNumberFromCallback(raw);
        PaymentDiagnosticContext.CurrentOrderNumber = orderNumber;

        var outcome = await _payments.ProcessCallbackAsync(raw, ActiveGatewayCode, cancellationToken);

        var resolvedOrder = !string.IsNullOrWhiteSpace(outcome.OrderNumber) ? outcome.OrderNumber : orderNumber;
        ViewBag.PaymentEvents = _diagnosticStore.GetForOrderWithGlobalFallback(resolvedOrder);
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

    [HttpGet]
    public async Task<IActionResult> Installments(string cardNumber, decimal amount = 0, CancellationToken cancellationToken = default)
    {
        var result = await _payments.GetInstallmentsAsync(cardNumber, amount, ActiveGatewayCode, cancellationToken);

        return Json(new
        {
            success = result.IsSuccess,
            installments = result.Data?.Installments ?? new List<TriPay.Services.Models.InstallmentOptionDto>()
        });
    }

    private static string? ResolveOrderNumberFromCallback(IReadOnlyDictionary<string, string> raw)
    {
        string[] keys = ["merchantPaymentId", "orderId", "OrderId", "VerifyEnrollmentRequestId", "SessionInfo"];
        foreach (var key in keys)
        {
            if (TryGetValueCaseInsensitive(raw, key, out var v) && !string.IsNullOrWhiteSpace(v))
                return v.Trim();
        }

        return raw.FirstOrDefault(x =>
            x.Key.Contains("order", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(x.Value)).Value?.Trim();
    }

    private static bool TryGetValueCaseInsensitive(
        IReadOnlyDictionary<string, string> raw,
        string key,
        out string value)
    {
        if (raw.TryGetValue(key, out value!))
            return true;

        foreach (var item in raw)
        {
            if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = item.Value;
                return true;
            }
        }

        value = string.Empty;
        return false;
    }
}

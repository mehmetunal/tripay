using TriPay.Core.Common;
using TriPay.Services;
using TriPay.Services.Checkout;
using TriPay.Services.Interfaces;
using TriPay.Services.Models;
using TriPay.Services.Diagnostics;
using TriPay.Services.Providers.VakifPays.Models;

namespace TriPay.Demo.Services;

/// <summary>
/// Framework modu demo orchestration: gateway çağrıları + üye işyeri sipariş deposu (TriPay MSSQL yok).
/// </summary>
public sealed class FrameworkDemoPaymentService
{
    private readonly IPaymentGatewayService _gateway;
    private readonly IDemoOrderStore _orders;

    public FrameworkDemoPaymentService(IPaymentGatewayService gateway, IDemoOrderStore orders)
    {
        _gateway = gateway;
        _orders = orders;
    }

    public async Task<Result<PaymentGatewayInitializeResponseDto>> PayAsync(
        PaymentRequest model,
        string gatewayName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.OrderNumber))
            model.OrderNumber = Guid.NewGuid().ToString("N");

        var existing = _orders.GetByOrderNumber(model.OrderNumber);
        if (existing is { Status: "Success" })
            return Result<PaymentGatewayInitializeResponseDto>.Failure("Bu sipariş numarası ile başarılı ödeme zaten var.");

        var record = existing ?? new DemoOrderRecord { OrderNumber = model.OrderNumber };
        record.Amount = model.Amount;
        record.Currency = model.Currency ?? "TRY";
        record.Status = "Pending";
        record.UpdatedAtUtc = DateTime.UtcNow;

        if (existing == null)
            _orders.Save(record);
        else
            _orders.Update(record);

        var initRequest = new PaymentGatewayInitializeRequestDto
        {
            GatewayName = gatewayName,
            Payment = model
        };

        var result = await _gateway.InitializePaymentAsync(initRequest, gatewayName);
        PaymentDiagnosticContext.CurrentOrderNumber = model.OrderNumber;

        if (result is { IsSuccess: true, Data.RedirectHtml: not null and not "" })
            PaymentDiagnostic.LogHtmlResponse(gatewayName, result.Data.RedirectHtml);

        if (result is { IsSuccess: true, Data: not null })
        {
            record.ExternalTransactionId = result.Data.PaymentId ?? result.Data.ConversationId;
            if (!result.Data.Success)
            {
                record.Status = "Failed";
                record.ResponseMessage = result.Data.Message;
            }
            _orders.Update(record);
        }

        return result;
    }

    public async Task<CheckoutCallbackResult> ProcessCallbackAsync(
        Dictionary<string, string> rawData,
        string gatewayName,
        CancellationToken cancellationToken = default)
    {
        var callbackResult = await _gateway.ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto
        {
            GatewayName = gatewayName,
            RawData = rawData
        }, gatewayName);

        var callback = callbackResult.Data ?? new PaymentGatewayCallbackResponseDto
        {
            ErrorMessage = callbackResult.ErrorMessage ?? string.Empty
        };

        var orderNumber = callback.OrderNumber;
        if (string.IsNullOrWhiteSpace(orderNumber))
            orderNumber = GetValueCaseInsensitive(rawData, "merchantPaymentId")
                ?? GetValueCaseInsensitive(rawData, "orderId")
                ?? GetValueCaseInsensitive(rawData, "OrderId")
                ?? GetValueCaseInsensitive(rawData, "VerifyEnrollmentRequestId")
                ?? GetValueCaseInsensitive(rawData, "SessionInfo")
                ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(orderNumber))
            PaymentDiagnosticContext.CurrentOrderNumber = orderNumber;

        var transaction = !string.IsNullOrWhiteSpace(orderNumber)
            ? _orders.GetByOrderNumber(orderNumber)
            : null;

        if (transaction == null)
        {
            return new CheckoutCallbackResult
            {
                Success = false,
                Message = "Sipariş kaydı bulunamadı (üye işyeri veritabanı).",
                OrderNumber = orderNumber,
                CallbackFieldsDisplay = string.Join(Environment.NewLine, rawData.Select(x => $"{x.Key}: {x.Value}"))
            };
        }

        var queryResult = await _gateway.GetPaymentStatusAsync(
            string.IsNullOrWhiteSpace(callback.OrderNumber) ? orderNumber : callback.OrderNumber,
            gatewayName);

        var query = queryResult.Data ?? new PaymentGatewayStatusResponseDto { ResponseCode = string.Empty };

        var amountText = rawData.GetValueOrDefault("amount") ?? string.Empty;
        var callbackAmountOk = true;
        if (decimal.TryParse(amountText.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var cbAmount))
        {
            callbackAmountOk = Math.Abs(cbAmount - transaction.Amount) < 0.01m;
        }

        var success = callbackAmountOk && callback.Success && query.Success && query.ResponseCode == "00";

        transaction.Status = success ? "Success" : "Failed";
        transaction.ResponseCode = callback.ResponseCode ?? query.ResponseCode;
        transaction.ResponseMessage = success ? "Ödeme doğrulandı." : callback.Message;
        transaction.ExternalTransactionId = callback.TransactionId ?? transaction.ExternalTransactionId;
        transaction.UpdatedAtUtc = DateTime.UtcNow;
        _orders.Update(transaction);

        return new CheckoutCallbackResult
        {
            Success = success,
            Message = transaction.ResponseMessage ?? (success ? "Ödeme başarılı." : "Ödeme başarısız."),
            OrderNumber = orderNumber,
            TransactionId = transaction.ExternalTransactionId,
            ResponseCode = transaction.ResponseCode ?? string.Empty,
            CallbackMessage = callback.Message ?? string.Empty,
            ErrorMessage = callback.ErrorMessage ?? string.Empty,
            QueryResponseCode = query.ResponseCode ?? string.Empty,
            CallbackFieldsDisplay = string.Join(Environment.NewLine, rawData.Select(x => $"{x.Key}: {x.Value}")),
            AmountText = amountText
        };
    }

    public async Task<Result<InstallmentInfoResponse>> GetInstallmentsAsync(
        string cardNumber,
        decimal amount,
        string gatewayName,
        CancellationToken cancellationToken = default)
    {
        var result = await _gateway.GetInstallmentInfoAsync(new PaymentGatewayInstallmentRequestDto
        {
            CardNumber = cardNumber,
            Amount = amount,
            GatewayName = gatewayName,
            TestPlatform = true
        }, gatewayName);

        if (!result.IsSuccess)
            return Result<InstallmentInfoResponse>.Failure(result.ErrorMessage ?? "Taksit sorgusu başarısız.");

        return Result<InstallmentInfoResponse>.Success(result.Data ?? new InstallmentInfoResponse());
    }

    private static string? GetValueCaseInsensitive(IReadOnlyDictionary<string, string> rawData, string key)
    {
        if (rawData.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            return value;

        foreach (var item in rawData)
        {
            if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(item.Value))
                return item.Value;
        }

        return null;
    }
}

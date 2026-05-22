using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using TriPay.Services;
using TriPay.Services.Common;
using TriPay.Services.Interfaces;
using TriPay.Services.Models;

namespace TriPay.Services.Providers;

public class VakifPaysGatewayProvider : PaymentGatewayBase
{
    private readonly VakifPaysService _vakifPaysService;

    public VakifPaysGatewayProvider(
        VakifPaysService vakifPaysService,
        ILogger<VakifPaysGatewayProvider> logger) : base(logger)
    {
        _vakifPaysService = vakifPaysService;
    }

    public override string GatewayName => PaymentGatewayNames.VakifPays;
    public override string DisplayName => "VakıfPayS";

    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request)
    {
        var supports3D = await _vakifPaysService.Is3DSupportedByCard(request.Payment.CardNumber, request.Payment.TestPlatform);
        if (!supports3D)
        {
            request.Payment.Use3D = false;
            var sale = await _vakifPaysService.Sale(request.Payment);
            return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
            {
                Success = sale.Success,
                Message = sale.Message,
                RedirectUrl = null,
                RedirectHtml = null
            });
        }

        var model = await _vakifPaysService.Get3DSecureUrl(request.Payment);
        var html = BuildAutoPostHtml(model.PostUrl, model.PostData);

        return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
        {
            Success = true,
            Message = "3D ödeme başlatıldı",
            RedirectHtml = html,
            RedirectUrl = model.PostUrl
        });
    }

    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request)
    {
        var success = request.RawData.TryGetValue("responseCode", out var code) && code == "00";
        request.RawData.TryGetValue("responseMsg", out var msg);
        request.RawData.TryGetValue("merchantPaymentId", out var order);
        request.RawData.TryGetValue("pgTranId", out var tran);

        var response = new PaymentGatewayCallbackResponseDto
        {
            Success = success,
            Message = msg ?? string.Empty,
            OrderNumber = order ?? string.Empty,
            TransactionId = tran ?? string.Empty,
            ResponseCode = code ?? string.Empty,
            ErrorMessage = request.RawData.GetValueOrDefault("errorMsg") ?? string.Empty
        };

        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(response));
    }

    public override async Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request)
    {
        var rawCard = string.IsNullOrWhiteSpace(request.CardNumber) ? request.BinNumber : request.CardNumber;
        var digits = new string((rawCard ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length < 6)
        {
            return Result<PaymentGatewayInstallmentResponseDto>.Failure("Geçersiz kart numarası.");
        }

        var result = await _vakifPaysService.BinInstallmentQuery(digits[..6], request.TestPlatform);
        var list = new List<InstallmentOptionDto>
        {
            new() { Count = 1, Rate = 0m, Total = request.Amount, Monthly = request.Amount, Label = $"1 (Tek Çekim) - {request.Amount:N2} TL" }
        };

        if (result.Raw != null && result.Raw.TryGetValue("installmentPaymentSystem", out var ipsObj) && ipsObj != null)
        {
            var root = JToken.FromObject(ipsObj);
            var installments = root["installmentList"] as JArray;
            if (installments != null)
            {
                foreach (var item in installments)
                {
                    var count = item["count"]?.Value<int>() ?? 0;
                    if (count <= 1) continue;
                    var rate = item["customerCostCommissionRate"]?.Value<decimal?>() ?? item["interestRate"]?.Value<decimal?>() ?? 0m;
                    var total = request.Amount * (1 + rate / 100m);
                    var monthly = total / count;
                    list.Add(new InstallmentOptionDto
                    {
                        Count = count,
                        Rate = rate,
                        Total = total,
                        Monthly = monthly,
                        Label = $"{count} Taksit - %{rate:N2} vade farkı - Aylık {monthly:N2} TL (Toplam {total:N2} TL)"
                    });
                }
            }
        }

        return Result<PaymentGatewayInstallmentResponseDto>.Success(new PaymentGatewayInstallmentResponseDto
        {
            Success = true,
            Installments = list.GroupBy(x => x.Count).Select(x => x.First()).OrderBy(x => x.Count).ToList()
        });
    }

    public override async Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
    {
        var query = await _vakifPaysService.QueryTransaction(paymentId);
        return Result<PaymentGatewayStatusResponseDto>.Success(new PaymentGatewayStatusResponseDto
        {
            Success = query.Success,
            Message = query.Message,
            ResponseCode = query.Raw?.GetValueOrDefault("responseCode")?.ToString() ?? string.Empty,
            Raw = query.Raw
        });
    }

    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request)
    {
        var callback = await ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto { RawData = request.RawData });
        if (!callback.IsSuccess || callback.Data == null)
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(callback.ErrorMessage ?? "3D callback işlenemedi.");

        return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
        {
            Success = callback.Data.Success,
            Message = callback.Data.Message,
            OrderNumber = callback.Data.OrderNumber,
            TransactionId = callback.Data.TransactionId,
            ResponseCode = callback.Data.ResponseCode,
            ErrorMessage = callback.Data.ErrorMessage
        });
    }

    public override async Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
    {
        var refund = await _vakifPaysService.Refund(new CancelRefundRequest
        {
            TransactionId = paymentId,
            Amount = amount ?? 0m
        });

        return Result<PaymentGatewayRefundResponseDto>.Success(new PaymentGatewayRefundResponseDto
        {
            Success = refund.Success,
            Message = refund.Message,
            Raw = refund.Raw
        });
    }

    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        rawData.TryGetValue("responseCode", out var status);
        rawData.TryGetValue("pgTranId", out var paymentId);
        rawData.TryGetValue("merchantPaymentId", out var conversationId);
        rawData.TryGetValue("responseMsg", out var paymentStatus);
        rawData.TryGetValue("errorCode", out var errorCode);
        rawData.TryGetValue("errorMsg", out var errorMessage);

        return (status, paymentId, conversationId, paymentStatus, errorCode, errorMessage);
    }

    private static string BuildAutoPostHtml(string url, Dictionary<string, string> data)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("<html><head><script>stringSubmit=function(){document.forms['vakifform'].submit();};</script></head>");
        sb.Append("<body onload='stringSubmit();'>");
        sb.Append($"<form id='vakifform' name='vakifform' action='{url}' method='POST'>");
        foreach (var item in data)
            sb.Append($"<input type='hidden' name='{item.Key}' value='{item.Value}' />");
        sb.Append("</form></body></html>");
        return sb.ToString();
    }
}

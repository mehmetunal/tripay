using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;

namespace TriPay.Services.Providers.Protocols.ApiV2;

/// <summary>
/// API v2 protokolünü kullanan gateway'ler için ortak taban.
/// Marka-agnostik: Paratika, Payten MSU, VakıfPayS, ZiraatPay aynı protokolü paylaşır.
/// </summary>
public abstract class ApiV2ProtocolGatewayBase : HttpPaymentGatewayBase
{
    private readonly ApiV2ProtocolClient _api;
    private bool _isTestMode;

    /// <summary>Protokol istemcisi.</summary>
    protected ApiV2ProtocolClient Api => _api;

    /// <summary>Aktif test modu.</summary>
    protected bool ProtocolTestMode => _isTestMode;

    /// <summary>Endpoint yapılandırması ile protokol taban sınıfını başlatır.</summary>
    protected ApiV2ProtocolGatewayBase(
        ApiV2EndpointConfig endpoints,
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
        _api = new ApiV2ProtocolClient(endpoints, (url, fields) => MakeFormRequestAsync(url, fields), DisplayName);
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

        var success = request.RawData.TryGetValue("responseCode", out var code) && code == "00";
        request.RawData.TryGetValue("responseMsg", out var msg);
        request.RawData.TryGetValue("merchantPaymentId", out var order);
        request.RawData.TryGetValue("pgTranId", out var tran);

        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = success,
            Message = msg ?? string.Empty,
            OrderNumber = order ?? string.Empty,
            TransactionId = tran ?? string.Empty,
            ResponseCode = code ?? string.Empty,
            ErrorMessage = request.RawData.GetValueOrDefault("errorMsg") ?? string.Empty
        }));
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
    {
        if (!await EnsureProtocolSettingsAsync())
            return Result<PaymentGatewayInstallmentResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

        var rawCard = string.IsNullOrWhiteSpace(request.CardNumber) ? (request.BinNumber ?? "") : request.CardNumber;
        var digits = PaymentCardHelper.DigitsOnly(rawCard);
        if (digits.Length < 6)
            return Result<PaymentGatewayInstallmentResponseDto>.Failure("Geçersiz kart numarası.");

        var result = await _api.QueryBinInstallmentsAsync(digits[..6], request.TestPlatform);
        var list = new List<InstallmentOptionDto>
        {
            new()
            {
                Count = 1,
                Rate = 0m,
                Total = request.Amount,
                Monthly = request.Amount,
                Label = $"1 (Tek Çekim) - {request.Amount:N2} TL"
            }
        };

        if (result.Raw != null && result.Raw.TryGetValue("installmentPaymentSystem", out var ipsObj) && ipsObj != null)
        {
            var root = JToken.FromObject(ipsObj);
            if (root["installmentList"] is JArray installments)
            {
                foreach (var item in installments)
                {
                    var count = item["count"]?.Value<int>() ?? 0;
                    if (count <= 1) continue;
                    var rate = item["customerCostCommissionRate"]?.Value<decimal?>()
                               ?? item["interestRate"]?.Value<decimal?>() ?? 0m;
                    var total = request.Amount * (1 + rate / 100m);
                    var monthly = total / count;
                    list.Add(new InstallmentOptionDto
                    {
                        Count = count,
                        Rate = rate,
                        Total = total,
                        Monthly = monthly,
                        Label =
                            $"{count} Taksit - %{rate:N2} vade farkı - Aylık {monthly:N2} TL (Toplam {total:N2} TL)"
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

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
    {
        if (!await EnsureProtocolSettingsAsync())
            return Result<PaymentGatewayStatusResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

        var query = await _api.QueryTransactionAsync(paymentId, _isTestMode);
        return Result<PaymentGatewayStatusResponseDto>.Success(new PaymentGatewayStatusResponseDto
        {
            Success = query.Success,
            Message = query.Message,
            ResponseCode = query.Raw?.GetValueOrDefault("responseCode")?.ToString() ?? string.Empty,
            Raw = query.Raw
        });
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
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

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(
        string paymentId,
        decimal? amount = null)
    {
        if (!await EnsureProtocolSettingsAsync())
            return Result<PaymentGatewayRefundResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

        var payload = _api.CreateAuthPayload();
        payload["ACTION"] = "REFUND";
        payload["PGTRANID"] = paymentId;
        payload["AMOUNT"] = BankAmountHelper.FormatTurkishDecimal(amount ?? 0m);
        payload["CURRENCY"] = "TRY";
        payload["REFLECTCOMMISSION"] = "No";

        var dic = await _api.PostFormAsync(payload, _isTestMode);
        return Result<PaymentGatewayRefundResponseDto>.Success(new PaymentGatewayRefundResponseDto
        {
            Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00",
            Message = dic.GetValueOrDefault("responseMsg")?.ToString() ?? "",
            Raw = dic
        });
    }

    /// <inheritdoc />
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode,
        string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        rawData.TryGetValue("responseCode", out var status);
        rawData.TryGetValue("pgTranId", out var paymentId);
        rawData.TryGetValue("merchantPaymentId", out var conversationId);
        rawData.TryGetValue("responseMsg", out var paymentStatus);
        rawData.TryGetValue("errorCode", out var errorCode);
        rawData.TryGetValue("errorMsg", out var errorMessage);
        return (status, paymentId, conversationId, paymentStatus, errorCode, errorMessage);
    }

    /// <summary>Protokol kimlik bilgilerini yükler.</summary>
    protected async Task<bool> EnsureProtocolSettingsAsync()
    {
        var config = await GetGatewayConfigAsync();
        if (config == null)
            return false;

        _isTestMode = config.IsTestMode;
        return _api.LoadSettings(config);
    }
}

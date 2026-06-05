using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;

namespace TriPay.Services.Providers.PayNKolay;

/// <summary>PayNKolay form POST + SHA512 hashDatav2 sanal POS entegrasyonu.</summary>
public sealed class PayNKolayGatewayProvider : HttpPaymentGatewayBase
{
    private const string ApiUrlTest = "https://paynkolaytest.nkolayislem.com.tr";
    private const string ApiUrlLive = "https://paynkolay.nkolayislem.com.tr";

    private string? _sx;
    private string? _storeKey;
    private bool _isTestMode;

    public PayNKolayGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<PayNKolayGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    public override string GatewayName => PaymentGatewayNames.PayNKolay;
    public override string DisplayName => "PayNKolay";

    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayInitializeResponseDto>.Failure("PayNKolay ayarları yüklenemedi.");

            var card = request.Payment;
            var totalStr = card.Amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))
                .Replace(".", "").Replace(",", ".");
            var rnd = DateTime.UtcNow.AddHours(3).ToString("dd.MM.yyyy HH:mm:ss");

            var form = new Dictionary<string, string>
            {
                ["sx"] = _sx!,
                ["clientRefCode"] = card.OrderNumber,
                ["amount"] = totalStr,
                ["installmentNo"] = (card.InstallmentCount > 1 ? card.InstallmentCount : 1).ToString(),
                ["cardHolderName"] = card.CardOwner,
                ["month"] = PaymentCardHelper.NormalizeMonth(card.ExpiryMonth),
                ["year"] = PaymentCardHelper.NormalizeYear(card.ExpiryYear),
                ["cvv"] = card.Cvv,
                ["cardNumber"] = PaymentCardHelper.DigitsOnly(card.CardNumber),
                ["transactionType"] = "SALES",
                ["rnd"] = rnd,
                ["environment"] = "API",
                ["currencyNumber"] = "949",
                ["cardHolderIP"] = card.CustomerIp,
                ["successUrl"] = card.ReturnUrl,
                ["failUrl"] = card.ReturnUrl,
                ["customerKey"] = "",
                ["use3D"] = "true"
            };

            var hashString = $"{form["sx"]}|{form["clientRefCode"]}|{form["amount"]}|{form["successUrl"]}|{form["failUrl"]}|{form["rnd"]}|{form["customerKey"]}|{_storeKey}";
            form["hashDatav2"] = BankHashHelper.Sha512Utf8Base64(hashString);

            var url = $"{ApiBase()}/Vpos/v1/Payment";
            var raw = await MakeFormRequestAsync(url, form);
            var dic = JsonResponseHelper.ParseDictionary(raw);

            if (dic.TryGetValue("RESPONSE_CODE", out var code) && code?.ToString() == "2"
                && dic.TryGetValue("USE_3D", out var use3d) && use3d?.ToString() == "true"
                && dic.TryGetValue("BANK_REQUEST_MESSAGE", out var html))
            {
                return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
                {
                    Success = true,
                    Message = "3D ödeme başlatıldı",
                    RedirectHtml = CleanHtml(html.ToString()!),
                    PaymentId = card.OrderNumber,
                    ConversationId = card.OrderNumber
                });
            }

            return Result<PaymentGatewayInitializeResponseDto>.Failure(
                dic.TryGetValue("RESPONSE_DATA", out var msg) ? msg?.ToString() ?? "3D başlatılamadı" : "3D başlatılamadı");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "PayNKolay InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");
        request.RawData.TryGetValue("CLIENT_REFERENCE_CODE", out var order);
        request.RawData.TryGetValue("RESPONSE_CODE", out var code);
        var success = code == "2";
        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = success,
            Message = success ? "3D callback alındı" : "3D callback başarısız",
            OrderNumber = order ?? string.Empty,
            PaymentStatus = success ? "PENDING" : "FAILED"
        }));
    }

    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("PayNKolay ayarları yüklenemedi.");

            request.RawData.TryGetValue("RESPONSE_CODE", out var code);
            request.RawData.TryGetValue("CLIENT_REFERENCE_CODE", out var order);
            request.RawData.TryGetValue("REFERENCE_CODE", out var referenceCode);

            if (code != "2" || string.IsNullOrWhiteSpace(referenceCode))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("3D doğrulama başarısız.");

            var form = new Dictionary<string, string>
            {
                ["sx"] = _sx!,
                ["referenceCode"] = referenceCode
            };

            var url = $"{ApiBase()}/Vpos/v1/CompletePayment";
            var raw = await MakeFormRequestAsync(url, form);
            var dic = JsonResponseHelper.ParseDictionary(raw);

            if (dic.TryGetValue("RESPONSE_CODE", out var c2) && c2?.ToString() == "2"
                && dic.TryGetValue("AUTH_CODE", out var auth) && !string.IsNullOrWhiteSpace(auth?.ToString()) && auth.ToString() != "0")
            {
                var transactionId = dic.TryGetValue("REFERENCE_CODE", out var refCode) ? refCode?.ToString() ?? "" : "";
                return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
                {
                    Success = true,
                    Message = "İşlem başarılı",
                    OrderNumber = order ?? string.Empty,
                    TransactionId = transactionId
                });
            }

            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                dic.TryGetValue("RESPONSE_DATA", out var msg) ? msg?.ToString() ?? "Ödeme tamamlanamadı" : "Ödeme tamamlanamadı");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "PayNKolay Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(ex.Message);
        }
    }

    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("PayNKolay durum sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("PayNKolay taksit sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("PayNKolay iade desteklenmiyor."));

    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode,
        string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        rawData.TryGetValue("RESPONSE_CODE", out var status);
        rawData.TryGetValue("REFERENCE_CODE", out var paymentId);
        rawData.TryGetValue("CLIENT_REFERENCE_CODE", out var conversationId);
        rawData.TryGetValue("RESPONSE_DATA", out var errorMessage);
        return (status, paymentId, conversationId, null, null, errorMessage);
    }

    private async Task<bool> InitializeSettingsAsync()
    {
        var config = await GetGatewayConfigAsync();
        if (config == null) return false;

        _isTestMode = config.IsTestMode;
        _sx = GatewaySettingsHelper.GetFirst(config, "MerchantId", "Sx");
        _storeKey = GatewaySettingsHelper.GetFirst(config, "StoreKey", "MerchantStorekey");

        return GatewaySettingsHelper.AllPresent(_sx, _storeKey);
    }

    private string ApiBase() => _isTestMode ? ApiUrlTest : ApiUrlLive;

    private static string CleanHtml(string input)
        => input.Replace("\\r", "").Replace("\\n", "").Replace("\r", "").Replace("\n", "")
            .Replace("\\\"", "\"").Trim();
}

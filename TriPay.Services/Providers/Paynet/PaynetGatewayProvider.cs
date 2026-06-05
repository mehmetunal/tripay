using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;

namespace TriPay.Services.Providers.Paynet;

/// <summary>Paynet JSON API + Basic auth sanal POS entegrasyonu.</summary>
public sealed class PaynetGatewayProvider : HttpPaymentGatewayBase
{
    private const string ApiUrlTest = "https://pts-api.paynet.com.tr";
    private const string ApiUrlLive = "https://api.paynet.com.tr";

    private string? _apiKey;
    private bool _isTestMode;

    public PaynetGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<PaynetGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    public override string GatewayName => PaymentGatewayNames.Paynet;
    public override string DisplayName => "Paynet";

    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Paynet ayarları yüklenemedi.");

            var card = request.Payment;
            var totalStr = card.Amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")).Replace(".", "");
            var host = TryGetHost(card.ReturnUrl);

            var body = new Dictionary<string, object>
            {
                ["amount"] = totalStr,
                ["reference_no"] = card.OrderNumber,
                ["return_url"] = card.ReturnUrl,
                ["domain"] = host,
                ["card_holder"] = card.CardOwner,
                ["pan"] = PaymentCardHelper.DigitsOnly(card.CardNumber),
                ["month"] = int.Parse(PaymentCardHelper.NormalizeMonth(card.ExpiryMonth)),
                ["year"] = int.Parse(PaymentCardHelper.NormalizeYear(card.ExpiryYear)),
                ["cvc"] = card.Cvv,
                ["card_holder_phone"] = card.CustomerPhone,
                ["card_holder_mail"] = card.CustomerEmail,
                ["instalment"] = card.InstallmentCount > 1 ? card.InstallmentCount : 1,
                ["add_commission"] = card.InstallmentCount > 1,
                ["transaction_type"] = 1
            };

            var url = $"{ApiBase()}/v2/transaction/tds_initial";
            var raw = await PostPaynetAsync(url, body);
            var dic = JsonResponseHelper.ParseDictionary(raw);

            if (dic.TryGetValue("code", out var code) && int.TryParse(code?.ToString(), out var codeInt)
                && (codeInt == 0 || codeInt == 100) && dic.TryGetValue("html_content", out var html))
            {
                return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
                {
                    Success = true,
                    Message = "3D ödeme başlatıldı",
                    RedirectHtml = html?.ToString(),
                    PaymentId = card.OrderNumber,
                    ConversationId = card.OrderNumber
                });
            }

            return Result<PaymentGatewayInitializeResponseDto>.Failure(
                dic.TryGetValue("message", out var msg) ? msg?.ToString() ?? "3D başlatılamadı" : "3D başlatılamadı");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Paynet InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");
        request.RawData.TryGetValue("session_id", out var sessionId);
        request.RawData.TryGetValue("token_id", out var tokenId);
        var pending = !string.IsNullOrWhiteSpace(sessionId) && !string.IsNullOrWhiteSpace(tokenId);
        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = pending,
            Message = pending ? "3D callback alındı" : "3D callback başarısız",
            PaymentStatus = pending ? "PENDING" : "FAILED"
        }));
    }

    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Paynet ayarları yüklenemedi.");

            request.RawData.TryGetValue("session_id", out var sessionId);
            request.RawData.TryGetValue("token_id", out var tokenId);
            if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(tokenId))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("3D oturum bilgisi eksik.");

            var body = new Dictionary<string, object>
            {
                ["session_id"] = sessionId,
                ["token_id"] = tokenId,
                ["transaction_type"] = 1
            };

            var url = $"{ApiBase()}/v2/transaction/tds_charge";
            var raw = await PostPaynetAsync(url, body);
            var dic = JsonResponseHelper.ParseDictionary(raw);

            if (dic.TryGetValue("is_succeed", out var ok) && ok is JsonElement el && el.GetBoolean())
            {
                var order = dic.TryGetValue("reference_no", out var refNo) ? refNo?.ToString() ?? "" : "";
                var transactionId = dic.TryGetValue("xact_id", out var xact) ? xact?.ToString() ?? "" : "";
                return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
                {
                    Success = true,
                    Message = "İşlem başarılı",
                    OrderNumber = order,
                    TransactionId = transactionId
                });
            }

            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                dic.TryGetValue("paynet_error_message", out var msg) ? msg?.ToString() ?? "Ödeme tamamlanamadı" : "Ödeme tamamlanamadı");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Paynet Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(ex.Message);
        }
    }

    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("Paynet durum sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("Paynet taksit sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("Paynet iade desteklenmiyor."));

    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode,
        string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        rawData.TryGetValue("session_id", out var sessionId);
        rawData.TryGetValue("token_id", out var tokenId);
        rawData.TryGetValue("paynet_error_message", out var errorMessage);
        return (sessionId, tokenId, null, null, null, errorMessage);
    }

    private async Task<bool> InitializeSettingsAsync()
    {
        var config = await GetGatewayConfigAsync();
        if (config == null) return false;

        _isTestMode = config.IsTestMode;
        _apiKey = GatewaySettingsHelper.GetFirst(config, "MerchantPassword", "ApiKey", "SecretKey", "Password");

        return !string.IsNullOrWhiteSpace(_apiKey);
    }

    private string ApiBase() => _isTestMode ? ApiUrlTest : ApiUrlLive;

    private async Task<string> PostPaynetAsync(string url, Dictionary<string, object> body)
    {
        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = $"Basic {_apiKey}"
        };
        return await MakeRequestAsyncRaw(url, HttpMethod.Post, JsonSerializer.Serialize(body), headers);
    }

    private static string TryGetHost(string returnUrl)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(returnUrl))
                return new Uri(returnUrl).Host;
        }
        catch
        {
            // yoksay
        }

        return "tripay.local";
    }

}

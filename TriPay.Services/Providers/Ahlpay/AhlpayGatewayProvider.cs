using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;

namespace TriPay.Services.Providers.Ahlpay;

/// <summary>Ahlpay token + SHA512 hash tabanlı sanal POS entegrasyonu.</summary>
public sealed class AhlpayGatewayProvider : HttpPaymentGatewayBase
{
    private const string ApiUrlTest = "https://testahlsanalpos.ahlpay.com.tr";
    private const string ApiUrlLive = "https://ahlsanalpos.ahlpay.com.tr";

    private string? _memberId;
    private string? _userCode;
    private string? _storeKey;
    private string? _email;
    private string? _password;
    private bool _isTestMode;

    public AhlpayGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<AhlpayGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    public override string GatewayName => PaymentGatewayNames.Ahlpay;
    public override string DisplayName => "Ahlpay";

    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Ahlpay ayarları yüklenemedi.");

            var card = request.Payment;
            var token = await GetTokenAsync();
            if (token == null)
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Ahlpay oturum token alınamadı.");

            var totalStr = card.Amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))
                .Replace(".", "").Replace(",", "");
            var rnd = $"RND{card.OrderNumber}";
            var hash = BankHashHelper.Sha512UnicodeHexUpper($"{_storeKey}{rnd}{card.OrderNumber}{totalStr}{token.MerchantId}");

            var body = new Dictionary<string, object>
            {
                ["cardNumber"] = PaymentCardHelper.DigitsOnly(card.CardNumber),
                ["expiryDateMonth"] = PaymentCardHelper.NormalizeMonth(card.ExpiryMonth),
                ["expiryDateYear"] = card.ExpiryYear,
                ["cvv"] = card.Cvv,
                ["cardHolderName"] = card.CardOwner,
                ["merchantId"] = token.MerchantId,
                ["totalAmount"] = totalStr,
                ["memberId"] = int.Parse(_memberId!),
                ["userCode"] = _userCode!,
                ["txnType"] = "Auth",
                ["installmentCount"] = card.InstallmentCount > 1 ? card.InstallmentCount.ToString() : "1",
                ["currency"] = "949",
                ["orderId"] = card.OrderNumber,
                ["webUrl"] = "",
                ["description"] = $"{card.OrderNumber} nolu sipariş ödemesi",
                ["requestIp"] = card.CustomerIp,
                ["rnd"] = rnd,
                ["hash"] = hash,
                ["okUrl"] = card.ReturnUrl,
                ["failUrl"] = card.ReturnUrl
            };

            var url = $"{ApiBase()}/api/Payment/Payment3DWithEventRedirect";
            var raw = await PostJsonAsync(url, body, token);
            var dic = JsonResponseHelper.ParseDictionary(raw);

            if (dic.TryGetValue("isSuccess", out var ok) && ok is JsonElement el && el.GetBoolean()
                && dic.TryGetValue("data", out var dataEl))
            {
                var html = dataEl.ToString();
                return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
                {
                    Success = true,
                    Message = "3D ödeme başlatıldı",
                    RedirectHtml = html,
                    PaymentId = card.OrderNumber,
                    ConversationId = card.OrderNumber
                });
            }

            return Result<PaymentGatewayInitializeResponseDto>.Failure(
                dic.TryGetValue("message", out var msg) ? msg?.ToString() ?? "3D başlatılamadı" : "3D başlatılamadı");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ahlpay InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");
        request.RawData.TryGetValue("orderId", out var orderId);
        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = true,
            Message = "3D callback alındı",
            OrderNumber = orderId ?? string.Empty,
            PaymentStatus = "PENDING"
        }));
    }

    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Ahlpay ayarları yüklenemedi.");

            request.RawData.TryGetValue("orderId", out var orderId);
            request.RawData.TryGetValue("rnd", out var rnd);
            if (string.IsNullOrWhiteSpace(orderId))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Sipariş numarası bulunamadı.");

            var token = await GetTokenAsync();
            if (token == null)
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Ahlpay oturum token alınamadı.");

            rnd = string.IsNullOrWhiteSpace(rnd) ? $"RND{orderId}" : rnd;
            var body = new Dictionary<string, object>
            {
                ["memberId"] = _memberId!,
                ["merchantId"] = token.MerchantId,
                ["hash"] = "",
                ["rnd"] = rnd,
                ["orderId"] = orderId
            };

            var url = $"{ApiBase()}/api/Payment/PaymentInquiry";
            var raw = await PostJsonAsync(url, body, token);
            var dic = JsonResponseHelper.ParseDictionary(raw);

            if (dic.TryGetValue("isSuccess", out var ok) && ok is JsonElement el && el.GetBoolean())
            {
                var transactionId = "";
                if (dic.TryGetValue("data", out var data) && data is JsonElement dataEl && dataEl.ValueKind == JsonValueKind.Object
                    && dataEl.TryGetProperty("authCode", out var authCode))
                    transactionId = authCode.GetString() ?? "";

                return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
                {
                    Success = true,
                    Message = "İşlem başarılı",
                    OrderNumber = orderId,
                    TransactionId = transactionId
                });
            }

            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                dic.TryGetValue("message", out var msg) ? msg?.ToString() ?? "Ödeme tamamlanamadı" : "Ödeme tamamlanamadı");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ahlpay Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(ex.Message);
        }
    }

    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("Ahlpay durum sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("Ahlpay taksit sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("Ahlpay iade desteklenmiyor."));

    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode,
        string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        rawData.TryGetValue("orderId", out var orderId);
        return (null, null, orderId, null, null, null);
    }

    private async Task<bool> InitializeSettingsAsync()
    {
        var config = await GetGatewayConfigAsync();
        if (config == null) return false;

        _isTestMode = config.IsTestMode;
        _memberId = GatewaySettingsHelper.GetFirst(config, "MerchantId", "MemberId");
        _userCode = GatewaySettingsHelper.GetFirst(config, "MerchantUser", "Username", "UserCode");
        _storeKey = GatewaySettingsHelper.GetFirst(config, "StoreKey", "MerchantStorekey");
        _email = GatewaySettingsHelper.GetFirst(config, "MerchantUser", "Email", "Username");
        _password = GatewaySettingsHelper.GetFirst(config, "MerchantPassword", "Password");

        return GatewaySettingsHelper.AllPresent(_memberId, _userCode, _storeKey, _email, _password);
    }

    private string ApiBase() => _isTestMode ? ApiUrlTest : ApiUrlLive;

    private async Task<AhlpayToken?> GetTokenAsync()
    {
        var body = new Dictionary<string, object>
        {
            ["email"] = _email!,
            ["password"] = _password!
        };
        var raw = await PostJsonAsync($"{ApiBase()}/api/Security/AuthenticationMerchant", body, null);
        var dic = JsonResponseHelper.ParseDictionary(raw);

        if (dic.TryGetValue("isSuccess", out var ok) && ok is JsonElement el && el.GetBoolean()
            && dic.TryGetValue("data", out var data))
        {
            var tokenJson = data!.ToString();
            return JsonSerializer.Deserialize<AhlpayToken>(tokenJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return null;
    }

    private async Task<string> PostJsonAsync(string url, Dictionary<string, object> body, AhlpayToken? token)
    {
        var headers = new Dictionary<string, string>();
        if (token != null && !string.IsNullOrWhiteSpace(token.Token))
            headers["Authorization"] = $"{token.TokenType} {token.Token}";

        return await MakeRequestAsyncRaw(url, HttpMethod.Post, JsonSerializer.Serialize(body), headers);
    }

    private sealed class AhlpayToken
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public long MerchantId { get; set; }
    }
}

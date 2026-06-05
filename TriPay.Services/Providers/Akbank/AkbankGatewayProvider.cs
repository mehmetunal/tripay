using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Akbank.Helpers;
using TriPay.Services.Providers.Nestpay.Helpers;

namespace TriPay.Services.Providers.Akbank;

/// <summary>Akbank native JSON API sanal POS entegrasyonu.</summary>
public sealed class AkbankGatewayProvider : HttpPaymentGatewayBase
{
    private const string ApiUrlTest = "https://apipre.akbank.com/api/v1/payment/virtualpos/transaction/process";
    private const string ApiUrlLive = "https://api.akbank.com/api/v1/payment/virtualpos/transaction/process";
    private const string ThreeDUrlTest = "https://virtualpospaymentgatewaypre.akbank.com/securepay";
    private const string ThreeDUrlLive = "https://virtualpospaymentgateway.akbank.com/securepay";
    private const string SuccessCode = "VPS-0000";

    private string? _merchantSafeId;
    private string? _terminalSafeId;
    private string? _storeKey;
    private bool _isTestMode;

    private static readonly Dictionary<string, int> CurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TRY"] = 949,
        ["USD"] = 840,
        ["EUR"] = 978,
        ["GBP"] = 826
    };

    /// <summary>Akbank provider örneği oluşturur.</summary>
    public AkbankGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<AkbankGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.Akbank;

    /// <inheritdoc />
    public override string DisplayName => "Akbank";

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync(request.Payment.TestPlatform))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Akbank ayarları yüklenemedi.");

            var card = request.Payment;
            var amount = FormatAmount(card.Amount);
            var currencyCode = ResolveCurrencyCode(card.Currency);
            var installment = card.InstallmentCount > 1 ? card.InstallmentCount : 1;
            var email = string.IsNullOrWhiteSpace(card.CustomerEmail) ? "test@test.com" : card.CustomerEmail;
            var randomNumber = AkbankHashHelper.GenerateRandomHex(128);
            var requestDateTime = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", CultureInfo.InvariantCulture);

            var formFields = new Dictionary<string, string>
            {
                ["paymentModel"] = "3D",
                ["txnCode"] = "3000",
                ["merchantSafeId"] = _merchantSafeId!,
                ["terminalSafeId"] = _terminalSafeId!,
                ["orderId"] = card.OrderNumber,
                ["lang"] = "TR",
                ["amount"] = amount,
                ["currencyCode"] = currencyCode.ToString(),
                ["installCount"] = installment.ToString(),
                ["okUrl"] = card.ReturnUrl,
                ["failUrl"] = card.ReturnUrl,
                ["emailAddress"] = email,
                ["creditCard"] = card.CardNumber.Replace(" ", ""),
                ["expiredDate"] = FormatExpiry(card.ExpiryMonth, card.ExpiryYear),
                ["cvv"] = card.Cvv,
                ["randomNumber"] = randomNumber,
                ["requestDateTime"] = requestDateTime
            };

            var hashInput = string.Concat(
                formFields["paymentModel"], formFields["txnCode"], formFields["merchantSafeId"],
                formFields["terminalSafeId"], formFields["orderId"], formFields["lang"], formFields["amount"],
                formFields["currencyCode"], formFields["installCount"], formFields["okUrl"], formFields["failUrl"],
                formFields["emailAddress"], formFields["creditCard"], formFields["expiredDate"], formFields["cvv"],
                formFields["randomNumber"], formFields["requestDateTime"]);

            formFields["hash"] = AkbankHashHelper.ComputeFormHash(hashInput, _storeKey!);

            var threeDUrl = _isTestMode ? ThreeDUrlTest : ThreeDUrlLive;
            PaymentDiagnostic.LogOutbound3DForm(GatewayName, threeDUrl, formFields, "Akbank 3D başlatma");

            var responseHtml = await MakeFormRequestAsync(threeDUrl, formFields);
            if (string.IsNullOrWhiteSpace(responseHtml))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("3D Secure başlatılamadı.");

            if (responseHtml.Contains($"action=\"{card.ReturnUrl}\"", StringComparison.OrdinalIgnoreCase))
            {
                var fields = NestpayXmlHelper.ParseFormFields(responseHtml);
                var errMsg = fields.GetValueOrDefault("responseMessage") ?? "3D Secure başlatılamadı.";
                return Result<PaymentGatewayInitializeResponseDto>.Failure(errMsg);
            }

            return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
            {
                Success = true,
                Message = "3D ödeme başlatıldı",
                PaymentId = card.OrderNumber,
                ConversationId = card.OrderNumber,
                RedirectHtml = responseHtml,
                RedirectUrl = threeDUrl
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Akbank InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

        var responseCode = GetRaw(request.RawData, "responseCode");
        var mdStatus = GetRaw(request.RawData, "mdStatus");
        var orderId = GetRaw(request.RawData, "orderId");

        var isSuccess = string.Equals(responseCode, SuccessCode, StringComparison.Ordinal)
                        && string.Equals(mdStatus, "1", StringComparison.Ordinal);

        if (!isSuccess)
        {
            var errMsg = GetRaw(request.RawData, "responseMessage") ?? "3D doğrulaması başarısız.";
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure(errMsg));
        }

        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = true,
            Message = "3D doğrulama başarılı",
            OrderNumber = orderId,
            PaymentStatus = "PENDING"
        }));
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Akbank ayarları yüklenemedi.");

            var raw = request.RawData;
            var responseCode = GetRaw(raw, "responseCode");
            var mdStatus = GetRaw(raw, "mdStatus");

            if (!string.Equals(responseCode, SuccessCode, StringComparison.Ordinal) ||
                !string.Equals(mdStatus, "1", StringComparison.Ordinal))
            {
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                    GetRaw(raw, "responseMessage") ?? "3D doğrulaması başarısız.");
            }

            var orderId = GetRaw(raw, "orderId") ?? request.ConversationId ?? request.PaymentId;
            var body = new Dictionary<string, object>
            {
                ["version"] = "1.00",
                ["txnCode"] = "1000",
                ["requestDateTime"] = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", CultureInfo.InvariantCulture),
                ["randomNumber"] = AkbankHashHelper.GenerateRandomHex(128),
                ["terminal"] = new Dictionary<string, object>
                {
                    ["merchantSafeId"] = _merchantSafeId!,
                    ["terminalSafeId"] = _terminalSafeId!
                },
                ["order"] = new Dictionary<string, object> { ["orderId"] = orderId! },
                ["transaction"] = new Dictionary<string, object>
                {
                    ["amount"] = GetRaw(raw, "amount") ?? "0",
                    ["currencyCode"] = ResolveCurrencyCode(GetRaw(raw, "currencyCode") ?? "TRY"),
                    ["motoInd"] = 0,
                    ["installCount"] = int.TryParse(GetRaw(raw, "installCount"), out var inst) ? inst : 1
                },
                ["secureTransaction"] = new Dictionary<string, object>
                {
                    ["secureId"] = GetRaw(raw, "secureId") ?? "",
                    ["secureEcomInd"] = GetRaw(raw, "secureEcomInd") ?? "",
                    ["secureData"] = GetRaw(raw, "secureData") ?? "",
                    ["secureMd"] = GetRaw(raw, "secureMd") ?? ""
                }
            };

            var response = await PostJsonAsync(body);
            if (response == null)
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Ödeme tamamlanamadı.");

            if (!string.Equals(GetJsonString(response, "responseCode"), SuccessCode, StringComparison.Ordinal))
            {
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                    GetJsonString(response, "responseMessage") ?? "Ödeme tamamlanamadı.");
            }

            var transactionId = ExtractAuthCode(response) ?? orderId;

            return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
            {
                Success = true,
                Status = "success",
                Message = "Ödeme tamamlandı",
                PaymentId = transactionId,
                PaymentStatus = "SUCCESS"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Akbank Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"Ödeme tamamlanırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(
        string paymentId, decimal? amount = null)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayRefundResponseDto>.Failure("Akbank ayarları yüklenemedi.");

            var txnCode = amount.HasValue ? "1002" : "1003";
            var body = new Dictionary<string, object>
            {
                ["version"] = "1.00",
                ["txnCode"] = txnCode,
                ["requestDateTime"] = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", CultureInfo.InvariantCulture),
                ["randomNumber"] = AkbankHashHelper.GenerateRandomHex(128),
                ["terminal"] = new Dictionary<string, object>
                {
                    ["merchantSafeId"] = _merchantSafeId!,
                    ["terminalSafeId"] = _terminalSafeId!
                },
                ["order"] = new Dictionary<string, object> { ["orderId"] = paymentId },
                ["customer"] = new Dictionary<string, object> { ["ipAddress"] = "127.0.0.1" }
            };

            if (amount.HasValue)
            {
                body["transaction"] = new Dictionary<string, object>
                {
                    ["amount"] = FormatAmount(amount.Value),
                    ["currencyCode"] = 949
                };
            }

            var response = await PostJsonAsync(body);
            if (response != null &&
                string.Equals(GetJsonString(response, "responseCode"), SuccessCode, StringComparison.Ordinal))
            {
                return Result<PaymentGatewayRefundResponseDto>.Success(new PaymentGatewayRefundResponseDto
                {
                    Success = true,
                    Message = "İade işlemi başarılı",
                    Raw = new Dictionary<string, object> { ["transactionId"] = paymentId }
                });
            }

            return Result<PaymentGatewayRefundResponseDto>.Failure(
                response != null ? GetJsonString(response, "responseMessage") ?? "İade başarısız." : "İade başarısız.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Akbank Refund exception");
            return Result<PaymentGatewayRefundResponseDto>.Failure($"İade sırasında hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure(
            "Akbank satış sorgusu şu an desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure(
            "Akbank taksit sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)
        NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        var responseCode = GetRaw(rawData, "responseCode");
        var orderId = GetRaw(rawData, "orderId");
        var isSuccess = string.Equals(responseCode, SuccessCode, StringComparison.Ordinal);

        return (
            responseCode,
            orderId,
            orderId,
            isSuccess ? "PENDING" : "FAILED",
            responseCode,
            GetRaw(rawData, "responseMessage"));
    }

    private async Task<bool> InitializeSettingsAsync(bool? forceTestMode = null)
    {
        var config = await GetGatewayConfigAsync();
        if (config is not { Enabled: true })
            return false;

        _isTestMode = forceTestMode ?? config.IsTestMode;
        _merchantSafeId = GetSetting(config, "Username") ?? GetSetting(config, "MerchantSafeId");
        _terminalSafeId = GetSetting(config, "Password") ?? GetSetting(config, "TerminalSafeId");
        _storeKey = GetSetting(config, "StoreKey");

        return !string.IsNullOrWhiteSpace(_merchantSafeId)
               && !string.IsNullOrWhiteSpace(_terminalSafeId)
               && !string.IsNullOrWhiteSpace(_storeKey);
    }

    private async Task<JsonDocument?> PostJsonAsync(Dictionary<string, object> body)
    {
        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var url = _isTestMode ? ApiUrlTest : ApiUrlLive;
        var headers = new Dictionary<string, string>
        {
            ["auth-hash"] = AkbankHashHelper.ComputeAuthHash(json, _storeKey!)
        };

        var raw = await MakeRequestAsyncRaw(url, HttpMethod.Post, json, headers, "application/json");
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        return JsonDocument.Parse(raw);
    }

    private static string FormatAmount(decimal amount)
        => amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))
            .Replace(".", "")
            .Replace(",", ".");

    private static int ResolveCurrencyCode(string currency)
    {
        if (int.TryParse(currency, out var numeric))
            return numeric;
        return CurrencyCodes.TryGetValue(currency, out var code) ? code : 949;
    }

    private static string FormatExpiry(string month, string year)
    {
        var m = new string(month.Where(char.IsDigit).ToArray()).PadLeft(2, '0');
        var y = new string(year.Where(char.IsDigit).ToArray());
        if (y.Length >= 4) y = y[^2..];
        return m + y.PadLeft(2, '0');
    }

    private static string? GetSetting(GatewayConfig config, string key)
        => config.Settings.TryGetValue(key, out var value) ? value : null;

    private static string? GetRaw(IReadOnlyDictionary<string, string> data, string key)
    {
        foreach (var kv in data)
        {
            if (kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        }
        return null;
    }

    private static string? GetJsonString(JsonDocument doc, string property)
        => doc.RootElement.TryGetProperty(property, out var el) ? el.GetString() : null;

    private static string? ExtractAuthCode(JsonDocument doc)
    {
        if (!doc.RootElement.TryGetProperty("transaction", out var transaction))
            return null;
        return transaction.TryGetProperty("authCode", out var auth) ? auth.GetString() : null;
    }
}

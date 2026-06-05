using System.Globalization;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;
using TriPay.Services.Providers.Nestpay.Helpers;

namespace TriPay.Services.Providers.Nestpay;

/// <summary>
/// Nestpay/EST protokolünü kullanan bankalar için ortak sanal POS provider tabanı.
/// Şablon B ayarları: MerchantId, Username, Password, StoreKey.
/// </summary>
public abstract class NestpayGatewayBase : HttpPaymentGatewayBase
{
    private readonly NestpayEndpointConfig _endpoints;

    private string? _merchantId;
    private string? _username;
    private string? _password;
    private string? _storeKey;
    private bool _isTestMode;

    private static readonly Dictionary<string, string> CurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TRY"] = "949",
        ["USD"] = "840",
        ["EUR"] = "978",
        ["GBP"] = "826"
    };

    /// <summary>Endpoint yapılandırması ve bağımlılıklarla Nestpay taban sınıfını başlatır.</summary>
    protected NestpayGatewayBase(
        NestpayEndpointConfig endpoints,
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
        _endpoints = endpoints;
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync(request.Payment.TestPlatform))
                return Result<PaymentGatewayInitializeResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

            var card = request.Payment;
            if (string.IsNullOrWhiteSpace(card.CardNumber) || card.CardNumber.Length < 13)
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Geçersiz kart numarası.");

            var installment = card.InstallmentCount > 1 ? card.InstallmentCount.ToString() : string.Empty;
            var amount = NestpayXmlHelper.FormatAmount(card.Amount);
            var currency = ResolveCurrencyCode(card.Currency);
            var cardNumber = card.CardNumber.Replace(" ", "");
            var expiryMonth = ParseExpiryPart(card.ExpiryMonth, 2);
            var expiryYear = ParseExpiryYear(card.ExpiryYear);

            var formParams = new Dictionary<string, string>
            {
                ["pan"] = cardNumber,
                ["cv2"] = card.Cvv,
                ["Ecom_Payment_Card_ExpDate_Year"] = expiryYear,
                ["Ecom_Payment_Card_ExpDate_Month"] = expiryMonth,
                ["clientid"] = _merchantId!,
                ["amount"] = amount,
                ["oid"] = card.OrderNumber,
                ["okUrl"] = card.ReturnUrl,
                ["failUrl"] = card.ReturnUrl,
                ["rnd"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                ["storetype"] = "3d",
                ["lang"] = "tr",
                ["currency"] = currency,
                ["installment"] = installment,
                ["taksit"] = installment,
                ["islemtipi"] = "Auth",
                ["hashAlgorithm"] = "ver3"
            };

            formParams["hash"] = NestpayHashHelper.ComputeVer3Hash(formParams, _storeKey!);

            var threeDUrl = _endpoints.Resolve3DUrl(_isTestMode);
            PaymentDiagnostic.LogOutbound3DForm(GatewayName, threeDUrl, formParams, "Nestpay EST 3D başlatma");

            var responseHtml = await MakeFormRequestAsync(threeDUrl, formParams);
            if (string.IsNullOrWhiteSpace(responseHtml))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("3D Secure başlatılamadı.");

            var formFields = NestpayXmlHelper.ParseFormFields(responseHtml);
            if (formFields.TryGetValue("Response", out var response) &&
                (response.Equals("Error", StringComparison.OrdinalIgnoreCase) ||
                 response.Equals("Decline", StringComparison.OrdinalIgnoreCase)))
            {
                var errMsg = formFields.GetValueOrDefault("ErrMsg") ?? "3D Secure başlatılamadı.";
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
            Logger.LogError(ex, "{Gateway} InitializePayment exception", GatewayName);
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

        if (request.RawData.Count == 0)
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure("Callback verisi boş."));

        var mdStatus = GetRaw(request.RawData, "mdStatus");
        var orderNumber = GetRaw(request.RawData, "oid") ?? GetRaw(request.RawData, "OrderId");

        if (!string.Equals(mdStatus, "1", StringComparison.Ordinal))
        {
            var errMsg = GetRaw(request.RawData, "ErrMsg")
                         ?? GetRaw(request.RawData, "mdErrorMsg")
                         ?? "3D doğrulaması başarısız.";
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure(errMsg));
        }

        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = true,
            Message = "3D doğrulama başarılı",
            OrderNumber = orderNumber,
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
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

            if (request.RawData.Count == 0)
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Auth3DS için callback verisi gerekli.");

            PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "Auth3DSAsync");

            var mdStatus = GetRaw(request.RawData, "mdStatus");
            if (!string.Equals(mdStatus, "1", StringComparison.Ordinal))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("3D doğrulaması başarısız.");

            var orderNumber = GetRaw(request.RawData, "oid")
                              ?? request.ConversationId
                              ?? request.PaymentId;
            var installment = GetRaw(request.RawData, "installment") ?? string.Empty;
            var clientIp = GetRaw(request.RawData, "clientIp") ?? "127.0.0.1";

            var xmlParams = new Dictionary<string, object?>
            {
                ["Name"] = _username,
                ["Password"] = _password,
                ["ClientId"] = _merchantId,
                ["IPAddress"] = clientIp,
                ["OrderId"] = orderNumber,
                ["Taksit"] = installment,
                ["Type"] = "Auth",
                ["Number"] = GetRaw(request.RawData, "md"),
                ["PayerTxnId"] = GetRaw(request.RawData, "xid"),
                ["PayerSecurityLevel"] = GetRaw(request.RawData, "eci"),
                ["PayerAuthenticationCode"] = GetRaw(request.RawData, "cavv")
            };

            var xml = NestpayXmlHelper.ToXml(xmlParams);
            var apiUrl = _endpoints.ResolveApiUrl(_isTestMode);
            var responseXml = await PostXmlAsync(apiUrl, xml);
            var parsed = NestpayXmlHelper.ParseResponse(responseXml);

            if (!parsed.TryGetValue("Response", out var response) ||
                !response.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                var errMsg = parsed.GetValueOrDefault("ErrMsg") ?? "Ödeme tamamlanamadı.";
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(errMsg);
            }

            var transactionId = parsed.GetValueOrDefault("TransId") ?? orderNumber;

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
            Logger.LogError(ex, "{Gateway} Auth3DS exception", GatewayName);
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
                return Result<PaymentGatewayRefundResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

            if (string.IsNullOrWhiteSpace(paymentId))
                return Result<PaymentGatewayRefundResponseDto>.Failure("İade için işlem kimliği gerekli.");

            var xmlParams = new Dictionary<string, object?>
            {
                ["Name"] = _username,
                ["Password"] = _password,
                ["ClientId"] = _merchantId,
                ["Type"] = amount.HasValue ? "Credit" : "Void",
                ["TransId"] = paymentId
            };

            if (amount.HasValue)
                xmlParams["Total"] = NestpayXmlHelper.FormatAmount(amount.Value);

            var xml = NestpayXmlHelper.ToXml(xmlParams);
            var apiUrl = _endpoints.ResolveApiUrl(_isTestMode);
            var responseXml = await PostXmlAsync(apiUrl, xml);
            var parsed = NestpayXmlHelper.ParseResponse(responseXml);

            if (parsed.TryGetValue("Response", out var response) &&
                response.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                return Result<PaymentGatewayRefundResponseDto>.Success(new PaymentGatewayRefundResponseDto
                {
                    Success = true,
                    Message = "İade işlemi başarılı",
                    Raw = new Dictionary<string, object> { ["transactionId"] = paymentId }
                });
            }

            var errMsg = parsed.GetValueOrDefault("ErrMsg") ?? "İade işlemi başarısız.";
            return Result<PaymentGatewayRefundResponseDto>.Failure(errMsg);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{Gateway} Refund exception", GatewayName);
            return Result<PaymentGatewayRefundResponseDto>.Failure($"İade sırasında hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayStatusResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

            var xmlParams = new Dictionary<string, object?>
            {
                ["Name"] = _username,
                ["Password"] = _password,
                ["ClientId"] = _merchantId,
                ["OrderId"] = paymentId,
                ["Extra"] = new Dictionary<string, object?> { ["ORDERSTATUS"] = "QUERY" }
            };

            var xml = NestpayXmlHelper.ToXml(xmlParams);
            var apiUrl = _endpoints.ResolveApiUrl(_isTestMode);
            var responseXml = await PostXmlAsync(apiUrl, xml);
            var parsed = NestpayXmlHelper.ParseResponse(responseXml);

            if (!parsed.TryGetValue("Response", out var response) ||
                !response.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                return Result<PaymentGatewayStatusResponseDto>.Failure(
                    parsed.GetValueOrDefault("ErrMsg") ?? "Sipariş bulunamadı.");
            }

            var status = "UNKNOWN";
            if (parsed.TryGetValue("Extra.TRANS_STAT", out var transStat))
            {
                status = transStat switch
                {
                    "S" => "SUCCESS",
                    "V" => "VOIDED",
                    _ => transStat
                };
            }

            return Result<PaymentGatewayStatusResponseDto>.Success(new PaymentGatewayStatusResponseDto
            {
                Success = true,
                PaymentId = parsed.GetValueOrDefault("TransId") ?? paymentId,
                PaymentStatus = status,
                Message = "İşlem bulundu"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{Gateway} GetPaymentStatus exception", GatewayName);
            return Result<PaymentGatewayStatusResponseDto>.Failure($"Durum sorgulanırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure(
            $"{DisplayName} taksit sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)
        NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        var mdStatus = GetRaw(rawData, "mdStatus");
        var orderId = GetRaw(rawData, "oid");
        var isSuccess = string.Equals(mdStatus, "1", StringComparison.Ordinal);

        return (
            mdStatus,
            orderId,
            orderId,
            isSuccess ? "PENDING" : "FAILED",
            GetRaw(rawData, "ProcReturnCode"),
            GetRaw(rawData, "ErrMsg") ?? GetRaw(rawData, "mdErrorMsg"));
    }

    /// <summary>Gateway ayarlarını yükler.</summary>
    protected async Task<bool> InitializeSettingsAsync(bool? forceTestMode = null)
    {
        var config = await GetGatewayConfigAsync();
        if (config is not { Enabled: true })
            return false;

        _isTestMode = forceTestMode ?? config.IsTestMode;
        _merchantId = GatewaySettingsHelper.Get(config, "MerchantId");
        _username = GatewaySettingsHelper.Get(config, "Username") ?? GatewaySettingsHelper.Get(config, "MerchantUser");
        _password = GatewaySettingsHelper.Get(config, "Password") ?? GatewaySettingsHelper.Get(config, "MerchantPassword");
        _storeKey = GatewaySettingsHelper.Get(config, "StoreKey");

        return !string.IsNullOrWhiteSpace(_merchantId)
               && !string.IsNullOrWhiteSpace(_username)
               && !string.IsNullOrWhiteSpace(_password)
               && !string.IsNullOrWhiteSpace(_storeKey);
    }

    private async Task<string> PostXmlAsync(string url, string xml)
    {
        var formBody = $"DATA={Uri.EscapeDataString(xml)}";
        return await MakeRequestAsyncRaw(url, HttpMethod.Post, formBody, null, "application/x-www-form-urlencoded");
    }

    private static string ResolveCurrencyCode(string currency)
        => CurrencyCodes.TryGetValue(currency, out var code) ? code : "949";

    private static string ParseExpiryPart(string value, int length)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length >= length)
            return digits[^length..];
        return digits.PadLeft(length, '0');
    }

    private static string ParseExpiryYear(string year)
    {
        var digits = new string(year.Where(char.IsDigit).ToArray());
        if (digits.Length >= 4)
            return digits[^2..];
        if (digits.Length == 2)
            return digits;
        return digits.PadLeft(2, '0');
    }

    private static string? GetRaw(IReadOnlyDictionary<string, string> data, string key)
    {
        foreach (var kv in data)
        {
            if (kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        }
        return null;
    }
}

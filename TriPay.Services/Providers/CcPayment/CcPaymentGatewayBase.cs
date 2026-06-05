using System.Text.Json;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;
using TriPay.Services.Providers.CcPayment.Helpers;

namespace TriPay.Services.Providers.CcPayment;

/// <summary>Sipay tipi CCPayment REST API taban sınıfı.</summary>
public abstract class CcPaymentGatewayBase(
    CcPaymentEndpointConfig endpoints,
    IGatewaySettingsProvider settingsProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger)
    : HttpPaymentGatewayBase(settingsProvider, httpClientFactory, logger)
{
    private string? _appId;
    private string? _appSecret;
    private string? _merchantKey;
    private bool _isTestMode;

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync(request.Payment.TestPlatform))
                return Result<PaymentGatewayInitializeResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

            var card = request.Payment;
            var token = await GetTokenAsync();
            if (token == null)
                return Result<PaymentGatewayInitializeResponseDto>.Failure("API token alınamadı.");

            var total = BankAmountHelper.FormatTurkishDecimal(card.Amount);
            var installment = card.InstallmentCount.ToString();
            var currency = card.Currency;

            var body = new Dictionary<string, object>
            {
                ["cc_holder_name"] = card.CardOwner,
                ["cc_no"] = card.CardNumber.Replace(" ", ""),
                ["expiry_month"] = card.ExpiryMonth,
                ["expiry_year"] = card.ExpiryYear,
                ["cvv"] = card.Cvv,
                ["currency_code"] = currency,
                ["installments_number"] = installment,
                ["invoice_id"] = card.OrderNumber,
                ["invoice_description"] = $"{card.OrderNumber} nolu sipariş ödemesi",
                ["name"] = SplitName(card.CustomerName).first,
                ["surname"] = SplitName(card.CustomerName).last,
                ["total"] = total,
                ["merchant_key"] = _merchantKey!,
                ["transaction_type"] = "Auth",
                ["items"] = new[]
                {
                    new { name = "Tahsilat", price = total, quantity = 1, description = "Tahsilat" }
                },
                ["hash_key"] = CcPaymentHashHelper.GenerateSaleHash(total, installment, currency, _merchantKey!, card.OrderNumber, _appSecret!),
                ["response_method"] = "POST",
                ["payment_completed_by"] = "merchant",
                ["ip"] = card.CustomerIp,
                ["cancel_url"] = card.ReturnUrl,
                ["return_url"] = card.ReturnUrl
            };

            var baseUrl = endpoints.Resolve(_isTestMode);
            var url = $"{baseUrl}/api/paySmart3D";
            PaymentDiagnostic.LogOutboundHttpPost(GatewayName, url, JsonSerializer.Serialize(body), "application/json", "(3D başlatma)");

            var responseHtml = await PostJsonWithBearerAsync(url, body, token);
            if (string.IsNullOrWhiteSpace(responseHtml))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("3D Secure başlatılamadı.");

            return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
            {
                Success = true,
                Message = "3D ödeme başlatıldı",
                PaymentId = card.OrderNumber,
                ConversationId = card.OrderNumber,
                RedirectHtml = responseHtml,
                RedirectUrl = url
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{Gateway} InitializePayment exception", GatewayName);
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        if (!await InitializeSettingsAsync())
            return Result<PaymentGatewayCallbackResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

        var invoiceId = FormGatewayResponseHelper.GetRaw(request.RawData, "invoice_id");
        var hashKey = FormGatewayResponseHelper.GetRaw(request.RawData, "hash_key");
        var mdStatus = FormGatewayResponseHelper.GetRaw(request.RawData, "md_status");

        if (!string.IsNullOrWhiteSpace(hashKey))
        {
            var validated = CcPaymentHashHelper.ValidateHash(hashKey, _appSecret!);
            if (validated.Count == 0 || (invoiceId != null && validated.All(v => v != invoiceId)))
                return Result<PaymentGatewayCallbackResponseDto>.Failure("Hash doğrulanamadı.");
        }

        if (!string.Equals(mdStatus, "1", StringComparison.Ordinal))
            return Result<PaymentGatewayCallbackResponseDto>.Failure("3D doğrulaması başarısız.");

        return Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = true,
            Message = "3D doğrulama başarılı",
            OrderNumber = invoiceId ?? string.Empty,
            PaymentStatus = "PENDING"
        });
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

            var raw = request.RawData;
            var hashKey = FormGatewayResponseHelper.GetRaw(raw, "hash_key");
            var invoiceId = FormGatewayResponseHelper.GetRaw(raw, "invoice_id") ?? request.ConversationId ?? request.PaymentId;
            var orderId = FormGatewayResponseHelper.GetRaw(raw, "order_id") ?? "";
            var mdStatus = FormGatewayResponseHelper.GetRaw(raw, "md_status");

            if (!string.IsNullOrWhiteSpace(hashKey))
            {
                var validated = CcPaymentHashHelper.ValidateHash(hashKey, _appSecret!);
                if (validated.Count == 0)
                    return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Hash doğrulanamadı.");
            }

            if (!string.Equals(mdStatus, "1", StringComparison.Ordinal))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("3D doğrulaması başarısız.");

            var token = await GetTokenAsync();
            if (token == null)
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("API token alınamadı.");

            var body = new Dictionary<string, object>
            {
                ["merchant_key"] = _merchantKey!,
                ["invoice_id"] = invoiceId!,
                ["order_id"] = orderId,
                ["status"] = "complete",
                ["app_lang"] = "tr",
                ["hash_key"] = CcPaymentHashHelper.GenerateCompleteHash(_merchantKey!, invoiceId!, orderId, "complete", _appSecret!)
            };

            var baseUrl = endpoints.Resolve(_isTestMode);
            var url = $"{baseUrl}/payment/complete";
            var responseJson = await PostJsonWithBearerAsync(url, body, token);
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("status_code", out var statusCode) && statusCode.GetInt32() == 100)
            {
                var transId = orderId;
                if (root.TryGetProperty("data", out var data) && data.TryGetProperty("auth_code", out var authCode))
                    transId = authCode.GetString() ?? transId;

                return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
                {
                    Success = true,
                    Status = "success",
                    Message = "Ödeme tamamlandı",
                    PaymentId = transId,
                    PaymentStatus = "SUCCESS"
                });
            }

            var err = root.TryGetProperty("status_description", out var desc) ? desc.GetString() : null;
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(err ?? "Ödeme tamamlanamadı.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{Gateway} Auth3DS exception", GatewayName);
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"Ödeme tamamlanırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure($"{DisplayName} iade işlemi henüz desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure($"{DisplayName} durum sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure($"{DisplayName} taksit sorgusu henüz desteklenmiyor."));

    /// <inheritdoc />
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)
        NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        var mdStatus = FormGatewayResponseHelper.GetRaw(rawData, "md_status");
        var invoiceId = FormGatewayResponseHelper.GetRaw(rawData, "invoice_id");
        var isSuccess = mdStatus == "1";
        return (mdStatus, invoiceId, invoiceId, isSuccess ? "PENDING" : "FAILED", mdStatus,
            FormGatewayResponseHelper.GetRaw(rawData, "status_description"));
    }

    private async Task<string?> GetTokenAsync()
    {
        var baseUrl = endpoints.Resolve(_isTestMode);
        var body = new Dictionary<string, string>
        {
            ["app_id"] = _appId!,
            ["app_secret"] = _appSecret!
        };

        var json = JsonSerializer.Serialize(body);
        var response = await MakeRequestAsyncRaw($"{baseUrl}/api/token", HttpMethod.Post, json, null, "application/json");
        if (string.IsNullOrWhiteSpace(response))
            return null;

        using var doc = JsonDocument.Parse(response);
        var root = doc.RootElement;
        if (root.TryGetProperty("status_code", out var code) && code.GetInt32() == 100
            && root.TryGetProperty("data", out var data) && data.TryGetProperty("token", out var tokenEl))
            return tokenEl.GetString();

        return null;
    }

    private async Task<string> PostJsonWithBearerAsync(string url, object body, string bearerToken)
    {
        var json = JsonSerializer.Serialize(body);
        var headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {bearerToken}" };
        return await MakeRequestAsyncRaw(url, HttpMethod.Post, json, headers, "application/json");
    }

    private async Task<bool> InitializeSettingsAsync(bool? forceTestMode = null)
    {
        var config = await GetGatewayConfigAsync();
        if (config is not { Enabled: true })
            return false;

        _isTestMode = forceTestMode ?? config.IsTestMode;
        _appId = GatewaySettingsHelper.GetFirst(config, "ApiKey", "AppId", "Username");
        _appSecret = GatewaySettingsHelper.GetFirst(config, "SecretKey", "AppSecret", "Password");
        _merchantKey = GatewaySettingsHelper.GetFirst(config, "MerchantKey", "MerchantId", "StoreKey");

        return !string.IsNullOrWhiteSpace(_appId)
               && !string.IsNullOrWhiteSpace(_appSecret)
               && !string.IsNullOrWhiteSpace(_merchantKey);
    }

    private static (string first, string last) SplitName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return ("[boş]", "[boş]");
        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => ("[boş]", "[boş]"),
            1 => (parts[0], "[boş]"),
            _ => (parts[0], parts[1])
        };
    }
}

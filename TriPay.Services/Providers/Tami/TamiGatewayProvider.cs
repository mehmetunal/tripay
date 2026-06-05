using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Tami.Helpers;
using TriPay.Services.Providers.Common;
using TriPay.Services.Providers.VakifPays.Models;

namespace TriPay.Services.Providers.Tami;

/// <summary>Tami JSON API + JWK imza tabanlı sanal POS entegrasyonu.</summary>
public sealed class TamiGatewayProvider : HttpPaymentGatewayBase
{
    private const string ApiUrlTest = "https://sandbox-paymentapi.tami.com.tr";
    private const string ApiUrlLive = "https://paymentapi.tami.com.tr";

    private string? _merchantId;
    private string? _merchantUser;
    private string? _merchantPassword;
    private string? _storeKey;
    private bool _isTestMode;

    public TamiGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<TamiGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    public override string GatewayName => PaymentGatewayNames.Tami;
    public override string DisplayName => "Tami";

    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Tami ayarları yüklenemedi.");

            var card = request.Payment;
            var saleDic = BuildSaleDictionary(card, includeCallback: true);
            saleDic["securityHash"] = TamiJwkHelper.GenerateSecurityHash(_merchantPassword!, new Dictionary<string, object>(saleDic));

            var url = $"{ApiBase()}/payment/auth";
            var raw = await PostTamiAsync(url, saleDic);
            var dic = JsonResponseHelper.ParseDictionary(raw);

            if (dic.TryGetValue("success", out var ok) && ok is JsonElement el && el.GetBoolean()
                && dic.TryGetValue("threeDSHtmlContent", out var htmlB64))
            {
                var html = Encoding.UTF8.GetString(Convert.FromBase64String(htmlB64.ToString()!));
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
                dic.TryGetValue("errorMessage", out var msg) ? msg?.ToString() ?? "3D başlatılamadı" : "3D başlatılamadı");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Tami InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");
        request.RawData.TryGetValue("orderId", out var orderId);
        var success = request.RawData.TryGetValue("success", out var s) && s == "true";
        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = success,
            Message = success ? "3D doğrulama başarılı" : "3D doğrulama başarısız",
            OrderNumber = orderId ?? string.Empty,
            PaymentStatus = success ? "PENDING" : "FAILED"
        }));
    }

    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Tami ayarları yüklenemedi.");

            request.RawData.TryGetValue("orderId", out var orderId);
            request.RawData.TryGetValue("success", out var successFlag);
            if (successFlag != "true" || string.IsNullOrWhiteSpace(orderId))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("3D doğrulama başarısız.");

            var saleDic = new Dictionary<string, object> { ["orderId"] = orderId };
            saleDic["securityHash"] = TamiJwkHelper.GenerateSecurityHash(_merchantPassword!, saleDic);

            var url = $"{ApiBase()}/payment/complete-3ds";
            var raw = await PostTamiAsync(url, saleDic);
            var dic = JsonResponseHelper.ParseDictionary(raw);

            if (dic.TryGetValue("success", out var ok) && ok is JsonElement el && el.GetBoolean())
            {
                var transactionId = "";
                if (dic.TryGetValue("bankReferenceNumber", out var refNo))
                    transactionId = refNo?.ToString() ?? "";

                return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
                {
                    Success = true,
                    Message = "İşlem başarılı",
                    OrderNumber = orderId,
                    TransactionId = transactionId
                });
            }

            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                dic.TryGetValue("errorMessage", out var msg) ? msg?.ToString() ?? "Ödeme tamamlanamadı" : "Ödeme tamamlanamadı");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Tami Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(ex.Message);
        }
    }

    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("Tami durum sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("Tami taksit sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("Tami iade desteklenmiyor."));

    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode,
        string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        rawData.TryGetValue("success", out var status);
        rawData.TryGetValue("orderId", out var orderId);
        rawData.TryGetValue("errorMessage", out var errorMessage);
        return (status, null, orderId, null, null, errorMessage);
    }

    private static Dictionary<string, object> BuildSaleDictionary(PaymentRequest card, bool includeCallback)
    {
        var dic = new Dictionary<string, object>
        {
            ["amount"] = Math.Round(card.Amount, 2),
            ["orderId"] = card.OrderNumber,
            ["currency"] = card.Currency,
            ["installmentCount"] = card.InstallmentCount > 1 ? card.InstallmentCount : 1,
            ["paymentGroup"] = "OTHER",
            ["card"] = new Dictionary<string, object>
            {
                ["holderName"] = card.CardOwner,
                ["cvv"] = card.Cvv,
                ["number"] = PaymentCardHelper.DigitsOnly(card.CardNumber),
                ["expireMonth"] = int.Parse(PaymentCardHelper.NormalizeMonth(card.ExpiryMonth)),
                ["expireYear"] = int.Parse(PaymentCardHelper.NormalizeYear(card.ExpiryYear))
            },
            ["buyer"] = new Dictionary<string, object>
            {
                ["buyerId"] = card.CustomerId,
                ["ipAddress"] = card.CustomerIp,
                ["name"] = card.CustomerName,
                ["surName"] = "",
                ["identityNumber"] = card.CustomerId,
                ["city"] = card.BillToCity,
                ["country"] = card.BillToCountry,
                ["zipCode"] = card.BillToPostalCode,
                ["emailAddress"] = card.CustomerEmail,
                ["phoneNumber"] = card.CustomerPhone,
                ["registrationAddress"] = card.BillToAddressLine
            },
            ["shippingAddress"] = new Dictionary<string, object>
            {
                ["address"] = card.ShipToAddressLine,
                ["city"] = card.ShipToCity,
                ["country"] = card.ShipToCountry,
                ["zipCode"] = card.ShipToPostalCode,
                ["phoneNumber"] = card.ShipToPhone
            },
            ["billingAddress"] = new Dictionary<string, object>
            {
                ["address"] = card.BillToAddressLine,
                ["city"] = card.BillToCity,
                ["country"] = card.BillToCountry,
                ["zipCode"] = card.BillToPostalCode,
                ["phoneNumber"] = card.BillToPhone
            }
        };

        if (includeCallback)
            dic["callbackUrl"] = card.ReturnUrl;

        return dic;
    }

    private async Task<bool> InitializeSettingsAsync()
    {
        var config = await GetGatewayConfigAsync();
        if (config == null) return false;

        _isTestMode = config.IsTestMode;
        _merchantId = GatewaySettingsHelper.GetFirst(config, "MerchantId", "Merchant");
        _merchantUser = GatewaySettingsHelper.GetFirst(config, "MerchantUser", "Username");
        _merchantPassword = GatewaySettingsHelper.GetFirst(config, "MerchantPassword", "Password");
        _storeKey = GatewaySettingsHelper.GetFirst(config, "StoreKey", "MerchantStorekey");

        return GatewaySettingsHelper.AllPresent(_merchantId, _merchantUser, _merchantPassword, _storeKey);
    }

    private string ApiBase() => _isTestMode ? ApiUrlTest : ApiUrlLive;

    private async Task<string> PostTamiAsync(string url, Dictionary<string, object> body)
    {
        var headers = new Dictionary<string, string>
        {
            ["PG-Auth-Token"] = TamiJwkHelper.BuildPgAuthToken(_merchantId!, _merchantUser!, _storeKey!),
            ["correlationId"] = $"Correlation{Guid.NewGuid():N}"
        };
        return await MakeRequestAsyncRaw(url, HttpMethod.Post, JsonSerializer.Serialize(body), headers);
    }

}

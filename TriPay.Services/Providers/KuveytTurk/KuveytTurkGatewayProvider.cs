using System.Net;
using System.Xml;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;
using TriPay.Services.Providers.Nestpay.Helpers;

namespace TriPay.Services.Providers.KuveytTurk;

/// <summary>Kuveyt Türk BOA XML sanal POS entegrasyonu.</summary>
public sealed class KuveytTurkGatewayProvider : HttpPaymentGatewayBase
{
    private const string ThreeDPayGateTest = "https://boatest.kuveytturk.com.tr/boa.virtualpos.services/Home/ThreeDModelPayGate";
    private const string ThreeDPayGateLive = "https://sanalpos.kuveytturk.com.tr/ServiceGateWay/Home/ThreeDModelPayGate";
    private const string ProvisionGateTest = "https://boatest.kuveytturk.com.tr/boa.virtualpos.services/Home/ThreeDModelProvisionGate";
    private const string ProvisionGateLive = "https://sanalpos.kuveytturk.com.tr/ServiceGateWay/Home/ThreeDModelProvisionGate";

    private string? _merchantId;
    private string? _customerId;
    private string? _userName;
    private string? _password;
    private bool _isTestMode;

    private static readonly Dictionary<string, int> CurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TRY"] = 949, ["USD"] = 840, ["EUR"] = 978, ["GBP"] = 826
    };

    /// <summary>Kuveyt Türk provider örneği oluşturur.</summary>
    public KuveytTurkGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<KuveytTurkGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.KuveytTurk;

    /// <inheritdoc />
    public override string DisplayName => "Kuveyt Türk";

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync(request.Payment.TestPlatform))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Kuveyt Türk ayarları yüklenemedi.");

            var card = request.Payment;
            var amount = BankAmountHelper.FormatCommaless(card.Amount);
            var installment = card.InstallmentCount > 1 ? card.InstallmentCount : 0;
            var currency = ResolveCurrency(card.Currency).ToString("0000");

            var xmlParams = new Dictionary<string, object?>
            {
                ["APIVersion"] = "TDV2.0.0",
                ["OkUrl"] = card.ReturnUrl,
                ["FailUrl"] = card.ReturnUrl,
                ["MerchantId"] = _merchantId,
                ["CustomerId"] = _customerId,
                ["UserName"] = _userName,
                ["CardNumber"] = card.CardNumber.Replace(" ", ""),
                ["CardExpireDateYear"] = ExpiryYear2(card.ExpiryYear),
                ["CardExpireDateMonth"] = Pad2(card.ExpiryMonth),
                ["CardCVV2"] = card.Cvv,
                ["CardHolderName"] = card.CardOwner,
                ["BatchID"] = 0,
                ["TransactionType"] = "Sale",
                ["InstallmentCount"] = installment,
                ["Amount"] = amount,
                ["DisplayAmount"] = amount,
                ["CurrencyCode"] = currency,
                ["MerchantOrderId"] = card.OrderNumber,
                ["TransactionSecurity"] = 3,
                ["DeviceData"] = new Dictionary<string, object?>
                {
                    ["DeviceChannel"] = "02",
                    ["ClientIP"] = card.CustomerIp
                },
                ["CardHolderData"] = new Dictionary<string, object?>
                {
                    ["BillAddrCity"] = card.BillToCity,
                    ["BillAddrCountry"] = "792",
                    ["BillAddrLine1"] = card.BillToAddressLine,
                    ["BillAddrPostCode"] = card.BillToPostalCode,
                    ["Email"] = string.IsNullOrWhiteSpace(card.CustomerEmail) ? "test@test.com" : card.CustomerEmail,
                    ["MobilePhone"] = new Dictionary<string, object?>
                    {
                        ["Cc"] = "90",
                        ["Subscriber"] = NormalizePhone(card.CustomerPhone)
                    }
                }
            };

            var hash = BankHashHelper.Sha1Base64(string.Concat(
                _merchantId, card.OrderNumber, amount, card.ReturnUrl, card.ReturnUrl,
                _userName, BankHashHelper.Sha1Base64(_password!)));
            xmlParams["HashData"] = hash;

            var xml = NestpayXmlHelper.ToXml(xmlParams, "KuveytTurkVPosMessage");
            var url = _isTestMode ? ThreeDPayGateTest : ThreeDPayGateLive;
            PaymentDiagnostic.LogOutboundHttpPost(GatewayName, url, xml, "application/xml", "(3D başlatma)");

            var responseHtml = await MakeRequestAsyncRaw(url, HttpMethod.Post, xml, null, "application/xml");
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
            Logger.LogError(ex, "Kuveyt Türk InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

        var authResponse = FormGatewayResponseHelper.GetRaw(request.RawData, "AuthenticationResponse");
        if (string.IsNullOrWhiteSpace(authResponse))
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure("Callback verisi eksik."));

        var decoded = WebUtility.UrlDecode(authResponse);
        var parsed = NestpayXmlHelper.ParseResponse(decoded, "VPosTransactionResponseContract");

        if (parsed.GetValueOrDefault("ResponseCode") != "00")
        {
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure(
                parsed.GetValueOrDefault("ResponseMessage") ?? "3D doğrulaması başarısız."));
        }

        var orderId = parsed.GetValueOrDefault("MerchantOrderId");
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
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Kuveyt Türk ayarları yüklenemedi.");

            var authResponse = FormGatewayResponseHelper.GetRaw(request.RawData, "AuthenticationResponse");
            if (string.IsNullOrWhiteSpace(authResponse))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Auth3DS verisi eksik.");

            var decoded = WebUtility.UrlDecode(authResponse);
            var authParsed = NestpayXmlHelper.ParseResponse(decoded, "VPosTransactionResponseContract");
            if (authParsed.GetValueOrDefault("ResponseCode") != "00")
            {
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                    authParsed.GetValueOrDefault("ResponseMessage") ?? "3D doğrulaması başarısız.");
            }

            var orderId = authParsed.GetValueOrDefault("MerchantOrderId") ?? request.ConversationId ?? request.PaymentId;
            var md = authParsed.GetValueOrDefault("MD") ?? "";
            var amount = authParsed.GetValueOrDefault("Amount") ?? "0";
            var installment = authParsed.GetValueOrDefault("InstallmentCount") ?? "0";
            var currency = authParsed.GetValueOrDefault("CurrencyCode") ?? "0949";

            var xmlParams = new Dictionary<string, object?>
            {
                ["APIVersion"] = "TDV2.0.0",
                ["MerchantId"] = _merchantId,
                ["CustomerId"] = _customerId,
                ["UserName"] = _userName,
                ["TransactionType"] = "Sale",
                ["InstallmentCount"] = installment,
                ["Amount"] = amount,
                ["CurrencyCode"] = currency,
                ["MerchantOrderId"] = orderId,
                ["TransactionSecurity"] = 3,
                ["KuveytTurkVPosAdditionalData"] = new Dictionary<string, object?>
                {
                    ["AdditionalData"] = new Dictionary<string, object?>
                    {
                        ["Key"] = "MD",
                        ["Data"] = md
                    }
                }
            };

            var hash = BankHashHelper.Sha1Base64(string.Concat(
                _merchantId, orderId, amount, _userName, BankHashHelper.Sha1Base64(_password!)));
            xmlParams["HashData"] = hash;

            var xml = NestpayXmlHelper.ToXml(xmlParams, "KuveytTurkVPosMessage");
            var url = _isTestMode ? ProvisionGateTest : ProvisionGateLive;
            var responseXml = await MakeRequestAsyncRaw(url, HttpMethod.Post, xml, null, "application/xml");
            var parsed = NestpayXmlHelper.ParseResponse(responseXml, "VPosTransactionResponseContract");

            if (parsed.GetValueOrDefault("ResponseCode") != "00")
            {
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                    parsed.GetValueOrDefault("ResponseMessage") ?? "Ödeme tamamlanamadı.");
            }

            var transId = $"{parsed.GetValueOrDefault("ProvisionNumber")}|{parsed.GetValueOrDefault("OrderId")}";
            return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
            {
                Success = true,
                Status = "success",
                Message = "Ödeme tamamlandı",
                PaymentId = transId,
                PaymentStatus = "SUCCESS"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Kuveyt Türk Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"Ödeme tamamlanırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("Kuveyt Türk iade işlemi henüz desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("Kuveyt Türk durum sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("Kuveyt Türk taksit sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)
        NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        var authResponse = FormGatewayResponseHelper.GetRaw(rawData, "AuthenticationResponse");
        if (string.IsNullOrWhiteSpace(authResponse))
            return (null, null, null, "FAILED", null, "AuthenticationResponse eksik");

        var parsed = NestpayXmlHelper.ParseResponse(WebUtility.UrlDecode(authResponse), "VPosTransactionResponseContract");
        var code = parsed.GetValueOrDefault("ResponseCode");
        var orderId = parsed.GetValueOrDefault("MerchantOrderId");
        var isSuccess = code == "00";
        return (code, orderId, orderId, isSuccess ? "PENDING" : "FAILED", code,
            parsed.GetValueOrDefault("ResponseMessage"));
    }

    private async Task<bool> InitializeSettingsAsync(bool? forceTestMode = null)
    {
        var config = await GetGatewayConfigAsync();
        if (config is not { Enabled: true })
            return false;

        _isTestMode = forceTestMode ?? config.IsTestMode;
        _merchantId = GetSetting(config, "MerchantId");
        _customerId = GetSetting(config, "StoreKey") ?? GetSetting(config, "CustomerId");
        _userName = GetSetting(config, "Username");
        _password = GetSetting(config, "Password");

        return !string.IsNullOrWhiteSpace(_merchantId)
               && !string.IsNullOrWhiteSpace(_customerId)
               && !string.IsNullOrWhiteSpace(_userName)
               && !string.IsNullOrWhiteSpace(_password);
    }

    private static string? GetSetting(GatewayConfig config, string key)
        => config.Settings.TryGetValue(key, out var value) ? value : null;

    private static int ResolveCurrency(string currency)
        => CurrencyCodes.TryGetValue(currency, out var code) ? code : 949;

    private static string Pad2(string value)
        => new string(value.Where(char.IsDigit).ToArray()).PadLeft(2, '0')[^2..];

    private static string ExpiryYear2(string year)
    {
        var y = new string(year.Where(char.IsDigit).ToArray());
        return y.Length >= 4 ? y[^2..] : y.PadLeft(2, '0');
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length > 10 ? digits[^10..] : digits;
    }
}

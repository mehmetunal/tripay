using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;
using TriPay.Services.Providers.Nestpay.Helpers;

namespace TriPay.Services.Providers.VakifKatilim;

/// <summary>Vakıf Katılım BOA XML sanal POS entegrasyonu.</summary>
public sealed class VakifKatilimGatewayProvider : HttpPaymentGatewayBase
{
    private const string ThreeDPayGateLive = "https://boa.vakifkatilim.com.tr/VirtualPOS.Gateway/Home/ThreeDModelPayGate";
    private const string ProvisionGateLive = "https://boa.vakifkatilim.com.tr/VirtualPOS.Gateway/Home/ThreeDModelProvisionGate";

    private string? _merchantId;
    private string? _customerId;
    private string? _userName;
    private string? _password;
    private bool _isTestMode;

    private static readonly Dictionary<string, int> CurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TRY"] = 949, ["USD"] = 840, ["EUR"] = 978, ["GBP"] = 826
    };

    /// <summary>Vakıf Katılım provider örneği oluşturur.</summary>
    public VakifKatilimGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<VakifKatilimGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.VakifKatilim;

    /// <inheritdoc />
    public override string DisplayName => "Vakıf Katılım";

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync(request.Payment.TestPlatform))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Vakıf Katılım ayarları yüklenemedi.");

            var card = request.Payment;
            var amount = BankAmountHelper.FormatCommaless(card.Amount);
            var installment = card.InstallmentCount > 1 ? card.InstallmentCount : 0;
            var currency = ResolveCurrency(card.Currency).ToString("0000");
            var hashPassword = BankHashHelper.Sha1Base64(_password!);

            var xmlParams = new Dictionary<string, object?>
            {
                ["OkUrl"] = card.ReturnUrl,
                ["FailUrl"] = card.ReturnUrl,
                ["MerchantId"] = _merchantId,
                ["CustomerId"] = _customerId,
                ["UserName"] = _userName,
                ["HashPassword"] = hashPassword,
                ["MerchantOrderId"] = card.OrderNumber,
                ["InstallmentCount"] = installment,
                ["Amount"] = amount,
                ["DisplayAmount"] = amount,
                ["APIVersion"] = "1.0.0",
                ["CardNumber"] = card.CardNumber.Replace(" ", ""),
                ["CardExpireDateYear"] = ExpiryYear2(card.ExpiryYear),
                ["CardExpireDateMonth"] = Pad2(card.ExpiryMonth),
                ["CardCVV2"] = card.Cvv,
                ["CardHolderName"] = card.CardOwner,
                ["PaymentType"] = 1,
                ["CurrencyCode"] = currency,
                ["FECCurrencyCode"] = currency,
                ["TransactionSecurity"] = 3
            };

            var hash = BankHashHelper.Sha1Base64(string.Concat(
                _merchantId, card.OrderNumber, amount, card.ReturnUrl, card.ReturnUrl, _userName, hashPassword));
            xmlParams["HashData"] = hash;

            var xml = NestpayXmlHelper.ToXml(xmlParams, "VPosMessageContract");
            PaymentDiagnostic.LogOutboundHttpPost(GatewayName, ThreeDPayGateLive, xml, "application/xml", "(3D başlatma)");

            var responseHtml = await MakeRequestAsyncRaw(ThreeDPayGateLive, HttpMethod.Post, xml, null, "application/xml");
            if (string.IsNullOrWhiteSpace(responseHtml))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("3D Secure başlatılamadı.");

            return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
            {
                Success = true,
                Message = "3D ödeme başlatıldı",
                PaymentId = card.OrderNumber,
                ConversationId = card.OrderNumber,
                RedirectHtml = responseHtml,
                RedirectUrl = ThreeDPayGateLive
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vakıf Katılım InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

        var responseCode = FormGatewayResponseHelper.GetRaw(request.RawData, "ResponseCode");
        var orderId = FormGatewayResponseHelper.GetRaw(request.RawData, "MerchantOrderId");

        if (responseCode != "00")
        {
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure(
                FormGatewayResponseHelper.GetRaw(request.RawData, "ResponseMessage") ?? "3D doğrulaması başarısız."));
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
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Vakıf Katılım ayarları yüklenemedi.");

            var raw = request.RawData;
            if (FormGatewayResponseHelper.GetRaw(raw, "ResponseCode") != "00")
            {
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                    FormGatewayResponseHelper.GetRaw(raw, "ResponseMessage") ?? "3D doğrulaması başarısız.");
            }

            var orderId = FormGatewayResponseHelper.GetRaw(raw, "MerchantOrderId")
                          ?? request.ConversationId ?? request.PaymentId;
            var md = FormGatewayResponseHelper.GetRaw(raw, "MD") ?? "";
            var amount = FormGatewayResponseHelper.GetRaw(raw, "Amount")
                         ?? BankAmountHelper.FormatCommaless(0);

            var xmlParams = new Dictionary<string, object?>
            {
                ["APIVersion"] = "",
                ["MerchantId"] = _merchantId,
                ["CustomerId"] = _customerId,
                ["UserName"] = _userName,
                ["TransactionType"] = "Sale",
                ["InstallmentCount"] = FormGatewayResponseHelper.GetRaw(raw, "InstallmentCount") ?? "0",
                ["Amount"] = amount,
                ["CurrencyCode"] = FormGatewayResponseHelper.GetRaw(raw, "CurrencyCode") ?? "0949",
                ["FECCurrencyCode"] = FormGatewayResponseHelper.GetRaw(raw, "CurrencyCode") ?? "0949",
                ["MerchantOrderId"] = orderId,
                ["TransactionSecurity"] = 3,
                ["PaymentType"] = 1,
                ["AdditionalData"] = new Dictionary<string, object?>
                {
                    ["AdditionalDataList"] = new Dictionary<string, object?>
                    {
                        ["VPosAdditionalData"] = new Dictionary<string, object?>
                        {
                            ["Key"] = "MD",
                            ["Data"] = md
                        }
                    }
                }
            };

            var hash = BankHashHelper.Sha1Base64(string.Concat(
                _merchantId, orderId, amount, _userName, BankHashHelper.Sha1Base64(_password!)));
            xmlParams["HashData"] = hash;

            var xml = NestpayXmlHelper.ToXml(xmlParams, "VPosMessageContract");
            var responseXml = await MakeRequestAsyncRaw(ProvisionGateLive, HttpMethod.Post, xml, null, "application/xml");
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
            Logger.LogError(ex, "Vakıf Katılım Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"Ödeme tamamlanırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("Vakıf Katılım iade işlemi henüz desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("Vakıf Katılım durum sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("Vakıf Katılım taksit sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)
        NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        var code = FormGatewayResponseHelper.GetRaw(rawData, "ResponseCode");
        var orderId = FormGatewayResponseHelper.GetRaw(rawData, "MerchantOrderId");
        var isSuccess = code == "00";
        return (code, orderId, orderId, isSuccess ? "PENDING" : "FAILED", code,
            FormGatewayResponseHelper.GetRaw(rawData, "ResponseMessage"));
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
}

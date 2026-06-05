using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;
using TriPay.Services.Providers.Nestpay.Helpers;

namespace TriPay.Services.Providers.Denizbank;

/// <summary>Denizbank inter-vpos 3DPay sanal POS entegrasyonu.</summary>
public sealed class DenizbankGatewayProvider(
    IGatewaySettingsProvider settingsProvider,
    IHttpClientFactory httpClientFactory,
    ILogger<DenizbankGatewayProvider> logger)
    : HttpPaymentGatewayBase(settingsProvider, httpClientFactory, logger)
{
    private const string ApiUrlTest = "https://test.inter-vpos.com.tr/mpi/Default.aspx";
    private const string ApiUrlLive = "https://inter-vpos.com.tr/mpi/Default.aspx";

    private string? _shopCode;
    private string? _userCode;
    private string? _userPass;
    private string? _storeKey;
    private bool _isTestMode;

    private static readonly Dictionary<string, string> CurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TRY"] = "949", ["USD"] = "840", ["EUR"] = "978", ["GBP"] = "826"
    };

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.Denizbank;

    /// <inheritdoc />
    public override string DisplayName => "Denizbank";

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync(request.Payment.TestPlatform))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Denizbank ayarları yüklenemedi.");

            var card = request.Payment;
            var amount = BankAmountHelper.FormatTurkishDecimal(card.Amount);
            var installment = card.InstallmentCount > 1 ? card.InstallmentCount.ToString() : "0";
            var rnd = Guid.NewGuid().ToString("N");

            var form = new Dictionary<string, string>
            {
                ["ShopCode"] = _shopCode!,
                ["PurchAmount"] = amount,
                ["Currency"] = ResolveCurrency(card.Currency),
                ["OrderId"] = card.OrderNumber,
                ["OkUrl"] = card.ReturnUrl,
                ["FailUrl"] = card.ReturnUrl,
                ["Rnd"] = rnd,
                ["TxnType"] = "Auth",
                ["InstallmentCount"] = installment,
                ["SecureType"] = "3DPay",
                ["Pan"] = card.CardNumber.Replace(" ", ""),
                ["Cvv2"] = card.Cvv,
                ["Expiry"] = FormatExpiry(card.ExpiryMonth, card.ExpiryYear)
            };

            var hashInput = string.Concat(
                form["ShopCode"], form["OrderId"], form["PurchAmount"], form["OkUrl"],
                form["FailUrl"], form["TxnType"], form["InstallmentCount"], form["Rnd"], _storeKey);
            form["Hash"] = BankHashHelper.Sha1Base64(hashInput);

            var url = _isTestMode ? ApiUrlTest : ApiUrlLive;
            PaymentDiagnostic.LogOutbound3DForm(GatewayName, url, form, "Denizbank 3DPay başlatma");

            var responseHtml = await MakeFormRequestAsync(url, form);
            if (string.IsNullOrWhiteSpace(responseHtml))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("3D Secure başlatılamadı.");

            var fields = NestpayXmlHelper.ParseFormFields(responseHtml);
            if (fields.ContainsKey("ErrorMessage") || fields.ContainsKey("ErrorCode"))
            {
                var err = $"{fields.GetValueOrDefault("ErrorCode")} - {fields.GetValueOrDefault("ErrorMessage")}".Trim(' ', '-');
                return Result<PaymentGatewayInitializeResponseDto>.Failure(
                    string.IsNullOrWhiteSpace(err) ? "3D Secure başlatılamadı." : err);
            }

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
            Logger.LogError(ex, "Denizbank InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

        var procCode = FormGatewayResponseHelper.GetRaw(request.RawData, "ProcReturnCode");
        var orderId = FormGatewayResponseHelper.GetRaw(request.RawData, "OrderId");

        if (!string.Equals(procCode, "00", StringComparison.Ordinal))
        {
            var err = FormGatewayResponseHelper.GetRaw(request.RawData, "ErrorMessage") ?? "3D doğrulaması başarısız.";
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure(err));
        }

        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = true,
            Message = "3D doğrulama başarılı",
            OrderNumber = orderId ?? string.Empty,
            PaymentStatus = "PENDING"
        }));
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "Auth3DSAsync");

        var procCode = FormGatewayResponseHelper.GetRaw(request.RawData, "ProcReturnCode");
        if (!string.Equals(procCode, "00", StringComparison.Ordinal))
        {
            var err = FormGatewayResponseHelper.GetRaw(request.RawData, "ErrorMessage") ?? "Ödeme tamamlanamadı.";
            return Task.FromResult(Result<PaymentGatewayAuth3DSResponseDto>.Failure(err));
        }

        var orderId = FormGatewayResponseHelper.GetRaw(request.RawData, "OrderId")
                      ?? request.ConversationId
                      ?? request.PaymentId;
        var transId = FormGatewayResponseHelper.GetRaw(request.RawData, "TransId") ?? orderId;

        return Task.FromResult(Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
        {
            Success = true,
            Status = "success",
            Message = "Ödeme tamamlandı",
            PaymentId = transId,
            PaymentStatus = "SUCCESS"
        }));
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(
        string paymentId, decimal? amount = null)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayRefundResponseDto>.Failure("Denizbank ayarları yüklenemedi.");

            var form = new Dictionary<string, string>
            {
                ["ShopCode"] = _shopCode!,
                ["UserCode"] = _userCode!,
                ["UserPass"] = _userPass!,
                ["orgOrderId"] = paymentId,
                ["TxnType"] = amount.HasValue ? "Refund" : "Void",
                ["SecureType"] = "NonSecure",
                ["Lang"] = "TR"
            };

            if (amount.HasValue)
            {
                form["PurchAmount"] = BankAmountHelper.FormatTurkishDecimal(amount.Value);
                form["Currency"] = "949";
            }

            var url = _isTestMode ? ApiUrlTest : ApiUrlLive;
            var raw = await MakeFormRequestAsync(url, form);
            var parsed = FormGatewayResponseHelper.ParseDelimitedResponse(raw);

            if (parsed.TryGetValue("ProcReturnCode", out var code) && code == "00")
            {
                return Result<PaymentGatewayRefundResponseDto>.Success(new PaymentGatewayRefundResponseDto
                {
                    Success = true,
                    Message = "İade işlemi başarılı",
                    Raw = new Dictionary<string, object> { ["transactionId"] = paymentId }
                });
            }

            return Result<PaymentGatewayRefundResponseDto>.Failure(
                parsed.GetValueOrDefault("ErrorMessage") ?? "İade işlemi başarısız.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Denizbank Refund exception");
            return Result<PaymentGatewayRefundResponseDto>.Failure($"İade sırasında hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("Denizbank durum sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("Denizbank taksit sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)
        NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        var procCode = FormGatewayResponseHelper.GetRaw(rawData, "ProcReturnCode");
        var orderId = FormGatewayResponseHelper.GetRaw(rawData, "OrderId");
        var isSuccess = string.Equals(procCode, "00", StringComparison.Ordinal);

        return (procCode, orderId, orderId, isSuccess ? "PENDING" : "FAILED", procCode,
            FormGatewayResponseHelper.GetRaw(rawData, "ErrorMessage"));
    }

    private async Task<bool> InitializeSettingsAsync(bool? forceTestMode = null)
    {
        var config = await GetGatewayConfigAsync();
        if (config is not { Enabled: true })
            return false;

        _isTestMode = forceTestMode ?? config.IsTestMode;
        _shopCode = GetSetting(config, "MerchantId") ?? GetSetting(config, "ShopCode");
        _userCode = GetSetting(config, "Username") ?? GetSetting(config, "UserCode");
        _userPass = GetSetting(config, "Password") ?? GetSetting(config, "UserPass");
        _storeKey = GetSetting(config, "StoreKey");

        return !string.IsNullOrWhiteSpace(_shopCode)
               && !string.IsNullOrWhiteSpace(_userCode)
               && !string.IsNullOrWhiteSpace(_userPass)
               && !string.IsNullOrWhiteSpace(_storeKey);
    }

    private static string? GetSetting(GatewayConfig config, string key)
        => config.Settings.TryGetValue(key, out var value) ? value : null;

    private static string ResolveCurrency(string currency)
        => CurrencyCodes.TryGetValue(currency, out var code) ? code : "949";

    private static string FormatExpiry(string month, string year)
    {
        var m = new string([.. month.Where(char.IsDigit)]).PadLeft(2, '0');
        var y = new string([.. year.Where(char.IsDigit)]);
        if (y.Length >= 4) y = y[^2..];
        return m + y.PadLeft(2, '0');
    }
}

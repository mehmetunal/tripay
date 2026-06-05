using System.Xml;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;
using TriPay.Services.Providers.Nestpay.Helpers;

namespace TriPay.Services.Providers.Garanti;

/// <summary>Garanti BBVA GVPS XML sanal POS entegrasyonu.</summary>
public sealed class GarantiGatewayProvider(
    IGatewaySettingsProvider settingsProvider,
    IHttpClientFactory httpClientFactory,
    ILogger<GarantiGatewayProvider> logger)
    : HttpPaymentGatewayBase(settingsProvider, httpClientFactory, logger)
{
    private const string ApiUrlTest = "https://sanalposprovtest.garantibbva.com.tr/VPServlet";
    private const string ApiUrlLive = "https://sanalposprov.garanti.com.tr/VPServlet";
    private const string ThreeDUrlTest = "https://sanalposprovtest.garantibbva.com.tr/servlet/gt3dengine";
    private const string ThreeDUrlLive = "https://sanalposprov.garanti.com.tr/servlet/gt3dengine";

    private string? _merchantId;
    private string? _terminalId;
    private string? _provPassword;
    private string? _storeKey;
    private bool _isTestMode;

    private static readonly Dictionary<string, int> CurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TRY"] = 949, ["USD"] = 840, ["EUR"] = 978, ["GBP"] = 826
    };

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.Garanti;

    /// <inheritdoc />
    public override string DisplayName => "Garanti BBVA";

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync(request.Payment.TestPlatform))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Garanti ayarları yüklenemedi.");

            var card = request.Payment;
            var amount = BankAmountHelper.FormatMinorUnits(card.Amount);
            var installment = card.InstallmentCount > 1 ? card.InstallmentCount.ToString() : "";

            var form = new Dictionary<string, string>
            {
                ["mode"] = _isTestMode ? "TEST" : "PROD",
                ["apiversion"] = "v0.01",
                ["version"] = "v0.01",
                ["secure3dsecuritylevel"] = "3D",
                ["terminalprovuserid"] = "PROVAUT",
                ["terminaluserid"] = "PROVAUT",
                ["terminalmerchantid"] = _merchantId!,
                ["terminalid"] = _terminalId!,
                ["txntype"] = "sales",
                ["txnamount"] = amount,
                ["txncurrencycode"] = ResolveCurrency(card.Currency).ToString(),
                ["txninstallmentcount"] = installment,
                ["customeripaddress"] = card.CustomerIp,
                ["customeremailaddress"] = string.IsNullOrWhiteSpace(card.CustomerEmail) ? "test@test.com" : card.CustomerEmail,
                ["orderid"] = card.OrderNumber,
                ["cardnumber"] = card.CardNumber.Replace(" ", ""),
                ["cardexpiredatemonth"] = Pad2(card.ExpiryMonth),
                ["cardexpiredateyear"] = ExpiryYear2(card.ExpiryYear),
                ["cardcvv2"] = card.Cvv,
                ["successurl"] = card.ReturnUrl,
                ["errorurl"] = card.ReturnUrl
            };

            var hashedPassword = ComputeGarantiPasswordHash(_provPassword!, _terminalId!);
            var hash = BankHashHelper.Sha1HexUpper(string.Concat(
                _terminalId, card.OrderNumber, amount, card.ReturnUrl, card.ReturnUrl,
                form["txntype"], installment, _storeKey, hashedPassword));
            form["secure3dhash"] = hash;

            var threeDUrl = _isTestMode ? ThreeDUrlTest : ThreeDUrlLive;
            PaymentDiagnostic.LogOutbound3DForm(GatewayName, threeDUrl, form, "Garanti 3D başlatma");

            var responseHtml = await MakeFormRequestAsync(threeDUrl, form);
            if (string.IsNullOrWhiteSpace(responseHtml))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("3D Secure başlatılamadı.");

            var normalized = responseHtml.Replace(" value =\"", " value=\"", StringComparison.Ordinal);
            var fields = NestpayXmlHelper.ParseFormFields(normalized);

            if (fields.TryGetValue("response", out var resp) &&
                resp.Equals("error", StringComparison.OrdinalIgnoreCase))
            {
                return Result<PaymentGatewayInitializeResponseDto>.Failure(
                    fields.GetValueOrDefault("errmsg") ?? "3D Secure başlatılamadı.");
            }

            return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
            {
                Success = true,
                Message = "3D ödeme başlatıldı",
                PaymentId = card.OrderNumber,
                ConversationId = card.OrderNumber,
                RedirectHtml = normalized,
                RedirectUrl = threeDUrl
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Garanti InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

        var mdStatus = FormGatewayResponseHelper.GetRaw(request.RawData, "mdstatus")
                       ?? FormGatewayResponseHelper.GetRaw(request.RawData, "mdStatus");
        var orderId = FormGatewayResponseHelper.GetRaw(request.RawData, "oid")
                      ?? FormGatewayResponseHelper.GetRaw(request.RawData, "orderid");

        if (!string.Equals(mdStatus, "1", StringComparison.Ordinal))
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure("3D doğrulaması başarısız."));

        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = true,
            Message = "3D doğrulama başarılı",
            OrderNumber = orderId ?? string.Empty,
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
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Garanti ayarları yüklenemedi.");

            var raw = request.RawData;
            var mdStatus = FormGatewayResponseHelper.GetRaw(raw, "mdstatus")
                           ?? FormGatewayResponseHelper.GetRaw(raw, "mdStatus");
            if (!string.Equals(mdStatus, "1", StringComparison.Ordinal))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("3D doğrulaması başarısız.");

            var orderId = FormGatewayResponseHelper.GetRaw(raw, "oid") ?? request.ConversationId ?? request.PaymentId;
            var amount = FormGatewayResponseHelper.GetRaw(raw, "txnamount") ?? "0";
            var installment = FormGatewayResponseHelper.GetRaw(raw, "txninstallmentcount") ?? "";
            var currency = FormGatewayResponseHelper.GetRaw(raw, "txncurrencycode") ?? "949";

            var xmlParams = new Dictionary<string, object?>
            {
                ["Mode"] = _isTestMode ? "TEST" : "PROD",
                ["Version"] = "v0.00",
                ["Terminal"] = new Dictionary<string, object?>
                {
                    ["ProvUserID"] = "PROVAUT",
                    ["HashData"] = ComputeSaleHash(orderId!, amount),
                    ["MerchantID"] = _merchantId,
                    ["UserID"] = "PROVAUT",
                    ["ID"] = _terminalId
                },
                ["Customer"] = new Dictionary<string, object?>
                {
                    ["IPAddress"] = FormGatewayResponseHelper.GetRaw(raw, "customeripaddress") ?? "127.0.0.1",
                    ["EmailAddress"] = FormGatewayResponseHelper.GetRaw(raw, "customeremailaddress") ?? "test@test.com"
                },
                ["Card"] = new Dictionary<string, object?> { ["Number"] = "", ["ExpireDate"] = "", ["CVV2"] = "" },
                ["Order"] = new Dictionary<string, object?> { ["OrderID"] = orderId, ["GroupID"] = "", ["Description"] = "" },
                ["Transaction"] = new Dictionary<string, object?>
                {
                    ["Type"] = "sales",
                    ["InstallmentCnt"] = installment,
                    ["Amount"] = amount,
                    ["CurrencyCode"] = currency,
                    ["CardholderPresentCode"] = 13,
                    ["MotoInd"] = "N",
                    ["Secure3D"] = new Dictionary<string, object?>
                    {
                        ["AuthenticationCode"] = FormGatewayResponseHelper.GetRaw(raw, "cavv"),
                        ["SecurityLevel"] = FormGatewayResponseHelper.GetRaw(raw, "eci"),
                        ["TxnID"] = FormGatewayResponseHelper.GetRaw(raw, "xid"),
                        ["Md"] = FormGatewayResponseHelper.GetRaw(raw, "md")
                    }
                }
            };

            var xml = NestpayXmlHelper.ToXml(xmlParams, "GVPSRequest");
            var apiUrl = _isTestMode ? ApiUrlTest : ApiUrlLive;
            var responseXml = await MakeRequestAsyncRaw(apiUrl, HttpMethod.Post, xml, null, "application/xml");
            var (code, errorMessage, retrefNum) = ParseGvpsResponse(responseXml);
            if (!string.Equals(code, "00", StringComparison.Ordinal))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(errorMessage ?? "Ödeme tamamlanamadı.");

            var retRef = retrefNum ?? orderId;
            return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
            {
                Success = true,
                Status = "success",
                Message = "Ödeme tamamlandı",
                PaymentId = retRef,
                PaymentStatus = "SUCCESS"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Garanti Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"Ödeme tamamlanırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("Garanti iade işlemi henüz desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("Garanti durum sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("Garanti taksit sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)
        NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        var mdStatus = FormGatewayResponseHelper.GetRaw(rawData, "mdstatus")
                       ?? FormGatewayResponseHelper.GetRaw(rawData, "mdStatus");
        var orderId = FormGatewayResponseHelper.GetRaw(rawData, "oid");
        var isSuccess = string.Equals(mdStatus, "1", StringComparison.Ordinal);
        return (mdStatus, orderId, orderId, isSuccess ? "PENDING" : "FAILED", mdStatus,
            FormGatewayResponseHelper.GetRaw(rawData, "errmsg"));
    }

    private string ComputeSaleHash(string orderId, string amount)
    {
        var hashedPassword = ComputeGarantiPasswordHash(_provPassword!, _terminalId!);
        return BankHashHelper.Sha1HexUpper(string.Concat(orderId, _terminalId, amount, hashedPassword));
    }

    private static string ComputeGarantiPasswordHash(string password, string terminalId)
    {
        var padded = long.TryParse(terminalId, out var tid) ? tid.ToString("000000000") : terminalId.PadLeft(9, '0');
        return BankHashHelper.Sha1HexUpper(password + padded);
    }

    private async Task<bool> InitializeSettingsAsync(bool? forceTestMode = null)
    {
        var config = await GetGatewayConfigAsync();
        if (config is not { Enabled: true })
            return false;

        _isTestMode = forceTestMode ?? config.IsTestMode;
        _merchantId = GetSetting(config, "MerchantId");
        _terminalId = GetSetting(config, "TerminalId");
        _provPassword = GetSetting(config, "ProvPassword") ?? GetSetting(config, "Password");
        _storeKey = GetSetting(config, "StoreKey");

        return !string.IsNullOrWhiteSpace(_merchantId)
               && !string.IsNullOrWhiteSpace(_terminalId)
               && !string.IsNullOrWhiteSpace(_provPassword)
               && !string.IsNullOrWhiteSpace(_storeKey);
    }

    private static string? GetSetting(GatewayConfig config, string key)
        => config.Settings.TryGetValue(key, out var value) ? value : null;

    private static int ResolveCurrency(string currency)
        => CurrencyCodes.TryGetValue(currency, out var code) ? code : 949;

    private static string Pad2(string value)
        => new string([.. value.Where(char.IsDigit)]).PadLeft(2, '0')[^2..];

    private static string ExpiryYear2(string year)
    {
        var y = new string([.. year.Where(char.IsDigit)]);
        return y.Length >= 4 ? y[^2..] : y.PadLeft(2, '0');
    }

    private static (string? Code, string? ErrorMessage, string? RetrefNum) ParseGvpsResponse(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return (null, "Boş yanıt", null);

        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var code = doc.SelectSingleNode("//GVPSResponse/Transaction/Response/Code")?.InnerText
                   ?? doc.SelectSingleNode("//Transaction/Response/Code")?.InnerText;
        var error = doc.SelectSingleNode("//GVPSResponse/Transaction/Response/ErrorMsg")?.InnerText
                      ?? doc.SelectSingleNode("//Transaction/Response/ErrorMsg")?.InnerText;
        var retRef = doc.SelectSingleNode("//GVPSResponse/Transaction/RetrefNum")?.InnerText
                     ?? doc.SelectSingleNode("//Transaction/RetrefNum")?.InnerText;
        return (code, error, retRef);
    }
}

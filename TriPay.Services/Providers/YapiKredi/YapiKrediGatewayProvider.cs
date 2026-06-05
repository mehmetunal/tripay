using System.Xml;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;

namespace TriPay.Services.Providers.YapiKredi;

/// <summary>Yapı Kredi Posnet XML sanal POS entegrasyonu.</summary>
public sealed class YapiKrediGatewayProvider(
    IGatewaySettingsProvider settingsProvider,
    IHttpClientFactory httpClientFactory,
    ILogger<YapiKrediGatewayProvider> logger)
    : HttpPaymentGatewayBase(settingsProvider, httpClientFactory, logger)
{
    private const string ApiUrlTest = "https://setmpos.ykb.com/PosnetWebService/XML";
    private const string ApiUrlLive = "https://posnet.yapikredi.com.tr/PosnetWebService/XML";
    private const string ThreeDUrlTest = "https://setmpos.ykb.com/3DSWebService/YKBPaymentService";
    private const string ThreeDUrlLive = "https://posnet.yapikredi.com.tr/3DSWebService/YKBPaymentService";

    private string? _merchantId;
    private string? _terminalId;
    private string? _posnetId;
    private string? _storeKey;
    private bool _isTestMode;

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.YapiKredi;

    /// <inheritdoc />
    public override string DisplayName => "Yapı Kredi";

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync(request.Payment.TestPlatform))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Yapı Kredi ayarları yüklenemedi.");

            var card = request.Payment;
            var orderNumber = PadOrderNumber(card.OrderNumber);
            var amount = BankAmountHelper.FormatMinorUnits(card.Amount);
            var installment = (card.InstallmentCount > 1 ? card.InstallmentCount : 1).ToString("00");
            var currency = ToYkbCurrency(card.Currency);
            var expiry = ExpiryYear2(card.ExpiryYear) + Pad2(card.ExpiryMonth);

            var oosXml = $"""
                <posnetRequest>
                  <mid>{_merchantId}</mid>
                  <tid>{_terminalId}</tid>
                  <oosRequestData>
                    <posnetid>{_posnetId}</posnetid>
                    <XID>{orderNumber}</XID>
                    <amount>{amount}</amount>
                    <currencyCode>{currency}</currencyCode>
                    <installment>{installment}</installment>
                    <tranType>Sale</tranType>
                    <cardHolderName>{XmlEscape(card.CardOwner)}</cardHolderName>
                    <ccno>{card.CardNumber.Replace(" ", "")}</ccno>
                    <expDate>{expiry}</expDate>
                    <cvc>{card.Cvv}</cvc>
                  </oosRequestData>
                </posnetRequest>
                """;

            var step1Url = _isTestMode ? ApiUrlTest : ApiUrlLive;
            var step1Response = await PostFormXmlAsync(step1Url, oosXml);
            var step1 = ParsePosnetResponse(step1Response);

            if (step1.GetValueOrDefault("approved") != "1")
            {
                return Result<PaymentGatewayInitializeResponseDto>.Failure(
                    step1.GetValueOrDefault("respText") ?? "3D Secure başlatılamadı.");
            }

            var data1 = step1.GetValueOrDefault("data1") ?? "";
            var data2 = step1.GetValueOrDefault("data2") ?? "";
            var sign = step1.GetValueOrDefault("sign") ?? "";

            var threeDForm = new Dictionary<string, string>
            {
                ["mid"] = _merchantId!,
                ["posnetID"] = _posnetId!,
                ["posnetData"] = data1,
                ["posnetData2"] = data2,
                ["digest"] = sign,
                ["merchantReturnURL"] = card.ReturnUrl,
                ["lang"] = "tr"
            };

            var threeDUrl = _isTestMode ? ThreeDUrlTest : ThreeDUrlLive;
            PaymentDiagnostic.LogOutbound3DForm(GatewayName, threeDUrl, threeDForm, "Yapı Kredi 3D başlatma");

            var html = PaymentAutoPostHtmlBuilder.Build(threeDUrl, threeDForm);

            return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
            {
                Success = true,
                Message = "3D ödeme başlatıldı",
                PaymentId = orderNumber,
                ConversationId = orderNumber,
                RedirectHtml = html,
                RedirectUrl = threeDUrl
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Yapı Kredi InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

        var mdStatus = FormGatewayResponseHelper.GetRaw(request.RawData, "mdStatus");
        var xid = FormGatewayResponseHelper.GetRaw(request.RawData, "Xid")
                  ?? FormGatewayResponseHelper.GetRaw(request.RawData, "xid");

        if (!string.Equals(mdStatus, "1", StringComparison.Ordinal))
        {
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure("3D doğrulaması başarısız."));
        }

        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = true,
            Message = "3D doğrulama başarılı",
            OrderNumber = xid ?? string.Empty,
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
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Yapı Kredi ayarları yüklenemedi.");

            var raw = request.RawData;
            var bankPacket = FormGatewayResponseHelper.GetRaw(raw, "BankPacket") ?? "";
            var merchantPacket = FormGatewayResponseHelper.GetRaw(raw, "MerchantPacket") ?? "";
            var sign = FormGatewayResponseHelper.GetRaw(raw, "Sign") ?? "";
            var xid = FormGatewayResponseHelper.GetRaw(raw, "Xid") ?? request.ConversationId ?? request.PaymentId;
            var amount = FormGatewayResponseHelper.GetRaw(raw, "Amount") ?? "0";
            var currency = ToYkbCurrency(FormGatewayResponseHelper.GetRaw(raw, "Currency") ?? "TRY");

            var firstHash = BankHashHelper.Sha256Base64($"{_storeKey};{_terminalId}");
            var mac = BankHashHelper.Sha256Base64($"{xid};{amount};{currency};{_merchantId};{firstHash}");

            var resolveXml = $"""
                <posnetRequest>
                  <mid>{_merchantId}</mid>
                  <tid>{_terminalId}</tid>
                  <oosResolveMerchantData>
                    <bankData>{bankPacket}</bankData>
                    <merchantData>{merchantPacket}</merchantData>
                    <sign>{sign}</sign>
                    <mac>{mac}</mac>
                  </oosResolveMerchantData>
                </posnetRequest>
                """;

            var apiUrl = _isTestMode ? ApiUrlTest : ApiUrlLive;
            var resolveResponse = await PostFormXmlAsync(apiUrl, resolveXml);
            var resolve = ParsePosnetResponse(resolveResponse);

            if (resolve.GetValueOrDefault("approved") == "0")
            {
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                    resolve.GetValueOrDefault("respText") ?? "3D doğrulaması başarısız.");
            }

            var mdStatus = resolve.GetValueOrDefault("mdStatus");
            if (mdStatus != "1" && !(_isTestMode && mdStatus == "9"))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("3D doğrulaması başarısız.");

            var finalizeXml = $"""
                <posnetRequest>
                  <mid>{_merchantId}</mid>
                  <tid>{_terminalId}</tid>
                  <oosTranData>
                    <bankData>{bankPacket}</bankData>
                    <mac>{mac}</mac>
                  </oosTranData>
                </posnetRequest>
                """;

            var finalizeResponse = await PostFormXmlAsync(apiUrl, finalizeXml);
            var finalize = ParsePosnetResponse(finalizeResponse);

            if (finalize.GetValueOrDefault("approved") != "1")
            {
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                    finalize.GetValueOrDefault("respText") ?? "Ödeme tamamlanamadı.");
            }

            var hostLogKey = finalize.GetValueOrDefault("hostlogkey") ?? xid;
            return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
            {
                Success = true,
                Status = "success",
                Message = "Ödeme tamamlandı",
                PaymentId = hostLogKey,
                PaymentStatus = "SUCCESS"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Yapı Kredi Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"Ödeme tamamlanırken hata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("Yapı Kredi iade işlemi henüz desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("Yapı Kredi durum sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("Yapı Kredi taksit sorgusu desteklenmiyor."));

    /// <inheritdoc />
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)
        NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        var mdStatus = FormGatewayResponseHelper.GetRaw(rawData, "mdStatus");
        var xid = FormGatewayResponseHelper.GetRaw(rawData, "Xid") ?? FormGatewayResponseHelper.GetRaw(rawData, "xid");
        var isSuccess = string.Equals(mdStatus, "1", StringComparison.Ordinal);
        return (mdStatus, xid, xid, isSuccess ? "PENDING" : "FAILED", mdStatus,
            FormGatewayResponseHelper.GetRaw(rawData, "respText"));
    }

    private async Task<string> PostFormXmlAsync(string url, string xml)
    {
        var form = new Dictionary<string, string> { ["xmldata"] = xml };
        return await MakeFormRequestAsync(url, form);
    }

    private static Dictionary<string, string> ParsePosnetResponse(string xml)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(xml))
            return result;

        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var root = doc.DocumentElement;
        if (root == null)
            return result;

        void Walk(XmlNode node, string prefix)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                    continue;
                if (child.HasChildNodes && child.FirstChild?.NodeType == XmlNodeType.Element)
                    Walk(child, string.IsNullOrEmpty(prefix) ? child.Name : $"{prefix}.{child.Name}");
                else
                    result[string.IsNullOrEmpty(prefix) ? child.Name : $"{prefix}.{child.Name}"] = child.InnerText.Trim();
            }
        }

        Walk(root, "");
        return result;
    }

    private async Task<bool> InitializeSettingsAsync(bool? forceTestMode = null)
    {
        var config = await GetGatewayConfigAsync();
        if (config is not { Enabled: true })
            return false;

        _isTestMode = forceTestMode ?? config.IsTestMode;
        _merchantId = GetSetting(config, "MerchantId");
        _terminalId = GetSetting(config, "TerminalId");
        _posnetId = GetSetting(config, "Password") ?? GetSetting(config, "PosnetId");
        _storeKey = GetSetting(config, "StoreKey");

        return !string.IsNullOrWhiteSpace(_merchantId)
               && !string.IsNullOrWhiteSpace(_terminalId)
               && !string.IsNullOrWhiteSpace(_posnetId)
               && !string.IsNullOrWhiteSpace(_storeKey);
    }

    private static string? GetSetting(GatewayConfig config, string key)
        => config.Settings.TryGetValue(key, out var value) ? value : null;

    private static string ToYkbCurrency(string currency) => currency.ToUpperInvariant() switch
    {
        "TRY" => "TL",
        "USD" => "US",
        "EUR" => "EU",
        _ => "TL"
    };

    private static string PadOrderNumber(string orderNumber, int length = 20)
    {
        if (orderNumber.Length >= length)
            return orderNumber[..length];
        return orderNumber.PadLeft(length, '0');
    }

    private static string Pad2(string value)
        => new string([.. value.Where(char.IsDigit)]).PadLeft(2, '0')[^2..];

    private static string ExpiryYear2(string year)
    {
        var y = new string([.. year.Where(char.IsDigit)]);
        return y.Length >= 4 ? y[^2..] : y.PadLeft(2, '0');
    }

    private static string XmlEscape(string value)
        => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}

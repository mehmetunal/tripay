using System.Globalization;
using System.Xml;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;

namespace TriPay.Services.Providers.ParamPos;

/// <summary>ParamPos SOAP XML sanal POS entegrasyonu.</summary>
public sealed class ParamPosGatewayProvider : HttpPaymentGatewayBase
{
    private const string ApiUrlTest = "https://testposws.param.com.tr/turkpos.ws/service_turkpos_prod.asmx";
    private const string ApiUrlLive = "https://posws.param.com.tr/turkpos.ws/service_turkpos_prod.asmx";

    private string? _clientCode;
    private string? _clientUsername;
    private string? _clientPassword;
    private string? _guid;
    private bool _isTestMode;

    public ParamPosGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<ParamPosGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    public override string GatewayName => PaymentGatewayNames.ParamPos;
    public override string DisplayName => "ParamPos";

    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync(request.Payment.TestPlatform))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("ParamPos ayarları yüklenemedi.");

            var card = request.Payment;
            var installment = card.InstallmentCount > 1 ? card.InstallmentCount : 1;
            var amount = card.Amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")).Replace(".", "");
            var hash = BankHashHelper.Sha1Base64(string.Concat(_clientCode, _guid, installment, amount, amount, card.OrderNumber));

            var soapBody = $"""
                <TP_WMD_UCD xmlns="https://turkpos.com.tr/">
                  <G>
                    <CLIENT_CODE>{_clientCode}</CLIENT_CODE>
                    <CLIENT_USERNAME>{_clientUsername}</CLIENT_USERNAME>
                    <CLIENT_PASSWORD>{_clientPassword}</CLIENT_PASSWORD>
                  </G>
                  <GUID>{_guid}</GUID>
                  <KK_Sahibi>{card.CardOwner}</KK_Sahibi>
                  <KK_No>{card.CardNumber.Replace(" ", "")}</KK_No>
                  <KK_SK_Ay>{Pad2(card.ExpiryMonth)}</KK_SK_Ay>
                  <KK_SK_Yil>{card.ExpiryYear}</KK_SK_Yil>
                  <KK_CVC>{card.Cvv}</KK_CVC>
                  <KK_Sahibi_GSM></KK_Sahibi_GSM>
                  <Hata_URL>{card.ReturnUrl}</Hata_URL>
                  <Basarili_URL>{card.ReturnUrl}</Basarili_URL>
                  <Siparis_ID>{card.OrderNumber}</Siparis_ID>
                  <Taksit>{installment}</Taksit>
                  <Islem_Tutar>{amount}</Islem_Tutar>
                  <Toplam_Tutar>{amount}</Toplam_Tutar>
                  <Islem_Hash>{hash}</Islem_Hash>
                  <Islem_Guvenlik_Tip>3D</Islem_Guvenlik_Tip>
                  <IPAdr>{card.CustomerIp}</IPAdr>
                </TP_WMD_UCD>
                """;

            var envelope = WrapSoapEnvelope("TP_WMD_UCD", soapBody);
            var url = _isTestMode ? ApiUrlTest : ApiUrlLive;
            var responseXml = await MakeRequestAsyncRaw(url, HttpMethod.Post, envelope, null, "text/xml; charset=utf-8");
            var parsed = ParseSoapResult(responseXml, "TP_WMD_UCDResult");

            var sonuc = parsed.GetValueOrDefault("Sonuc");
            var html = parsed.GetValueOrDefault("UCD_HTML") ?? "";
            if (sonuc != null && int.TryParse(sonuc, out var sonucInt) && sonucInt > 0 && !string.IsNullOrWhiteSpace(html) && html != "NONSECURE")
            {
                return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
                {
                    Success = true,
                    Message = "3D ödeme başlatıldı",
                    PaymentId = card.OrderNumber,
                    ConversationId = card.OrderNumber,
                    RedirectHtml = html,
                    RedirectUrl = url
                });
            }

            return Result<PaymentGatewayInitializeResponseDto>.Failure(
                parsed.GetValueOrDefault("Sonuc_Str") ?? "3D Secure başlatılamadı.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ParamPos InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");
        var mdStatus = FormGatewayResponseHelper.GetRaw(request.RawData, "mdStatus");
        var orderId = FormGatewayResponseHelper.GetRaw(request.RawData, "orderId");

        if (mdStatus != "1")
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure("3D doğrulaması başarısız."));

        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = true,
            Message = "3D doğrulama başarılı",
            OrderNumber = orderId,
            PaymentStatus = "PENDING"
        }));
    }

    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("ParamPos ayarları yüklenemedi.");

            var mdStatus = FormGatewayResponseHelper.GetRaw(request.RawData, "mdStatus");
            if (mdStatus != "1")
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("3D doğrulaması başarısız.");

            var orderId = FormGatewayResponseHelper.GetRaw(request.RawData, "orderId") ?? request.ConversationId ?? request.PaymentId;
            var md = FormGatewayResponseHelper.GetRaw(request.RawData, "md") ?? "";
            var islemGuid = FormGatewayResponseHelper.GetRaw(request.RawData, "islemGUID") ?? "";

            var soapBody = $"""
                <TP_WMD_Pay xmlns="https://turkpos.com.tr/">
                  <G>
                    <CLIENT_CODE>{_clientCode}</CLIENT_CODE>
                    <CLIENT_USERNAME>{_clientUsername}</CLIENT_USERNAME>
                    <CLIENT_PASSWORD>{_clientPassword}</CLIENT_PASSWORD>
                  </G>
                  <GUID>{_guid}</GUID>
                  <UCD_MD>{md}</UCD_MD>
                  <Islem_GUID>{islemGuid}</Islem_GUID>
                  <Siparis_ID>{orderId}</Siparis_ID>
                </TP_WMD_Pay>
                """;

            var envelope = WrapSoapEnvelope("TP_WMD_Pay", soapBody);
            var url = _isTestMode ? ApiUrlTest : ApiUrlLive;
            var responseXml = await MakeRequestAsyncRaw(url, HttpMethod.Post, envelope, null, "text/xml; charset=utf-8");
            var parsed = ParseSoapResult(responseXml, "TP_WMD_PayResult");

            if (parsed.TryGetValue("Sonuc", out var sonuc) && int.TryParse(sonuc, out var s) && s > 0
                && parsed.TryGetValue("Dekont_ID", out var dekont) && long.TryParse(dekont, out var d) && d > 0)
            {
                return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
                {
                    Success = true,
                    Status = "success",
                    Message = "Ödeme tamamlandı",
                    PaymentId = dekont,
                    PaymentStatus = "SUCCESS"
                });
            }

            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                parsed.GetValueOrDefault("Sonuc_Ack") ?? "Ödeme tamamlanamadı.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ParamPos Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"Ödeme tamamlanırken hata: {ex.Message}");
        }
    }

    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("ParamPos iade henüz desteklenmiyor."));

    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("ParamPos durum sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("ParamPos taksit sorgusu desteklenmiyor."));

    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)
        NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        var mdStatus = FormGatewayResponseHelper.GetRaw(rawData, "mdStatus");
        var orderId = FormGatewayResponseHelper.GetRaw(rawData, "orderId");
        var ok = mdStatus == "1";
        return (mdStatus, orderId, orderId, ok ? "PENDING" : "FAILED", mdStatus, null);
    }

    private static string WrapSoapEnvelope(string action, string body)
        => $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                           xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                           xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                {body}
              </soap:Body>
            </soap:Envelope>
            """;

    private static Dictionary<string, string> ParseSoapResult(string xml, string resultNode)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(xml))
            return result;

        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var nodes = doc.GetElementsByTagName(resultNode);
        if (nodes.Count == 0)
            return result;

        foreach (XmlNode child in nodes[0]!.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element)
                result[child.Name] = child.InnerText.Trim();
        }

        return result;
    }

    private async Task<bool> InitializeSettingsAsync(bool? forceTestMode = null)
    {
        var config = await GetGatewayConfigAsync();
        if (config is not { Enabled: true })
            return false;

        _isTestMode = forceTestMode ?? config.IsTestMode;
        _clientCode = GetSetting(config, "MerchantId") ?? GetSetting(config, "ClientCode");
        _clientUsername = GetSetting(config, "Username") ?? GetSetting(config, "ClientUsername");
        _clientPassword = GetSetting(config, "Password") ?? GetSetting(config, "ClientPassword");
        _guid = GetSetting(config, "StoreKey") ?? GetSetting(config, "Guid");

        return !string.IsNullOrWhiteSpace(_clientCode)
               && !string.IsNullOrWhiteSpace(_clientUsername)
               && !string.IsNullOrWhiteSpace(_clientPassword)
               && !string.IsNullOrWhiteSpace(_guid);
    }

    private static string? GetSetting(GatewayConfig config, string key)
        => config.Settings.TryGetValue(key, out var value) ? value : null;

    private static string Pad2(string value)
        => new string(value.Where(char.IsDigit).ToArray()).PadLeft(2, '0')[^2..];
}

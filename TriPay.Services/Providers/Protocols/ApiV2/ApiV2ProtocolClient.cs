using Newtonsoft.Json;
using TriPay.Core.Gateways;
using TriPay.Services.Providers.Common;
using TriPay.Services.Providers.Protocols.ApiV2.Models;
using TriPay.Services.Providers.VakifPays.Models;

namespace TriPay.Services.Providers.Protocols.ApiV2;

/// <summary>
/// API v2 HTTP protokolü (SESSIONTOKEN, SALE, REFUND, sorgular).
/// Kanal provider'ları bu sınıfı composition ile kullanır; marka adı taşımaz.
/// </summary>
public sealed class ApiV2ProtocolClient
{
    private readonly ApiV2EndpointConfig _endpoints;
    private readonly Func<string, Dictionary<string, string>, Task<string>> _postFormAsync;
    private readonly string _displayName;

    private string? _merchantUser;
    private string? _merchantPassword;
    private string? _merchantCode;

    /// <summary>Endpoint ve form POST delegesi ile protokol istemcisini oluşturur.</summary>
    public ApiV2ProtocolClient(
        ApiV2EndpointConfig endpoints,
        Func<string, Dictionary<string, string>, Task<string>> postFormAsync,
        string displayName)
    {
        _endpoints = endpoints;
        _postFormAsync = postFormAsync;
        _displayName = displayName;
    }

    /// <summary>Gateway yapılandırmasından kimlik bilgilerini yükler.</summary>
    public bool LoadSettings(GatewayConfig config)
    {
        _merchantUser = GatewaySettingsHelper.GetFirst(config, "MerchantUser", "Username", "ApiUser");
        _merchantPassword = GatewaySettingsHelper.GetFirst(config, "MerchantPassword", "Password");
        _merchantCode = GatewaySettingsHelper.GetFirst(config, "Merchant", "MerchantId");
        return GatewaySettingsHelper.AllPresent(_merchantUser, _merchantPassword, _merchantCode);
    }

    /// <summary>Protokol kimlik alanlarını döndürür.</summary>
    public Dictionary<string, string> CreateAuthPayload()
        => new()
        {
            ["MERCHANTUSER"] = _merchantUser!,
            ["MERCHANTPASSWORD"] = _merchantPassword!,
            ["MERCHANT"] = _merchantCode!
        };

    /// <summary>Oturum / satış için temel form alanlarını oluşturur.</summary>
    public Dictionary<string, string> CreateBasePayload(PaymentRequest request)
    {
        var payload = CreateAuthPayload();
        payload["MERCHANTPAYMENTID"] = request.OrderNumber;
        payload["CUSTOMER"] = request.CustomerId;
        payload["CUSTOMERNAME"] = request.CustomerName;
        payload["CUSTOMEREMAIL"] = request.CustomerEmail;
        payload["CUSTOMERIP"] = request.CustomerIp;
        payload["CUSTOMERPHONE"] = request.CustomerPhone;
        payload["RETURNURL"] = request.ReturnUrl;
        payload["BILLTOADDRESSLINE"] = request.BillToAddressLine;
        payload["BILLTOCITY"] = request.BillToCity;
        payload["BILLTOCOUNTRY"] = request.BillToCountry;
        payload["BILLTOPOSTALCODE"] = request.BillToPostalCode;
        payload["BILLTOPHONE"] = request.BillToPhone;
        payload["SHIPTOADDRESSLINE"] = request.ShipToAddressLine;
        payload["SHIPTOCITY"] = request.ShipToCity;
        payload["SHIPTOCOUNTRY"] = request.ShipToCountry;
        payload["SHIPTOPOSTALCODE"] = request.ShipToPostalCode;
        payload["SHIPTOPHONE"] = request.ShipToPhone;
        payload["AMOUNT"] = BankAmountHelper.FormatTurkishDecimal(request.Amount);
        payload["CURRENCY"] = request.Currency;
        return payload;
    }

    /// <summary>Form POST gönderir ve JSON cevabı parse eder.</summary>
    public async Task<Dictionary<string, object>> PostFormAsync(Dictionary<string, string> payload, bool testPlatform)
    {
        var raw = await _postFormAsync(ApiUrl(testPlatform), payload);
        return JsonResponseHelper.ParseNewtonsoftDictionary(raw);
    }

    /// <summary>SESSIONTOKEN üretir.</summary>
    public async Task<string> CreateSessionTokenAsync(PaymentRequest model)
    {
        var payload = CreateBasePayload(model);
        payload["ACTION"] = "SESSIONTOKEN";
        payload["SESSIONTYPE"] = "PAYMENTSESSION";
        payload["ORDERITEMS"] =
            "[{\"code\":\"POSCEK\",\"name\":\"Cari Tahsilat\",\"description\":\"CariTahsilat\",\"quantity\":1,\"amount\":" +
            BankAmountHelper.FormatTurkishDecimal(model.Amount) + "}]";

        var result = await PostFormAsync(payload, model.TestPlatform);
        if (result.GetValueOrDefault("responseCode")?.ToString() == "00" && result.ContainsKey("sessionToken"))
            return result["sessionToken"]?.ToString() ?? "";

        throw new InvalidOperationException($"{_displayName} oturum hatası: {JsonConvert.SerializeObject(result)}");
    }

    /// <summary>BIN taksit / ödeme sistemi sorgusu.</summary>
    public async Task<ApiV2InstallmentQueryResponse> QueryBinInstallmentsAsync(string bin, bool testPlatform)
    {
        var payload = CreateAuthPayload();
        payload["ACTION"] = "QUERYPAYMENTSYSTEMS";
        payload["BIN"] = bin;

        var dic = await PostFormAsync(payload, testPlatform);
        return new ApiV2InstallmentQueryResponse
        {
            Raw = dic,
            Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00"
        };
    }

    /// <summary>İşlem durum sorgusu.</summary>
    public async Task<ApiV2SaleQueryResponse> QueryTransactionAsync(string merchantPaymentId, bool testPlatform)
    {
        var payload = CreateAuthPayload();
        payload["ACTION"] = "QUERYTRANSACTION";
        payload["MERCHANTPAYMENTID"] = merchantPaymentId;
        var dic = await PostFormAsync(payload, testPlatform);
        return new ApiV2SaleQueryResponse
        {
            Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00",
            Message = dic.GetValueOrDefault("responseMsg")?.ToString() ?? "",
            Raw = dic
        };
    }

    /// <summary>3D sale URL şablonunu doldurur.</summary>
    public string BuildSale3DUrl(string sessionToken, bool testPlatform)
        => string.Format(Sale3DUrlTemplate(testPlatform), sessionToken);

    /// <summary>3D HTML'e isteğe bağlı fraud script ekler.</summary>
    public string AppendFraudMetrixScript(string html, string sessionToken)
    {
        if (string.IsNullOrWhiteSpace(_endpoints.FraudMetrixOrgId))
            return html;

        var script = $@"
    <script type=""text/javascript"" src=""https://h.online-metrix.net/fp/tags.js?org_id={_endpoints.FraudMetrixOrgId}&amp;session_id={sessionToken}&pageid=1""></script>
    <noscript>
        <iframe style=""width: 100px; height: 100px; border: 0; position: absolute; top: -5000px;"" src=""https://h.online-metrix.net/fp/tags.js?org_id={_endpoints.FraudMetrixOrgId}&amp;session_id={sessionToken}&pageid=1""></iframe>
    </noscript>
";

        if (html.Contains("</body>", StringComparison.OrdinalIgnoreCase))
            return html.Replace("</body>", script + "</body>", StringComparison.OrdinalIgnoreCase);

        return html + script;
    }

    private string ApiUrl(bool testPlatform)
        => testPlatform ? _endpoints.ApiUrlTest : _endpoints.ApiUrlLive;

    private string Sale3DUrlTemplate(bool testPlatform)
        => testPlatform ? _endpoints.Sale3DUrlTestTemplate : _endpoints.Sale3DUrlLiveTemplate;
}

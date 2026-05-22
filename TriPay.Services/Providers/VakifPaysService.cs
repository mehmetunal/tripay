using System.Globalization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TriPay.Services.Providers;

public class VakifPaysService
{
    public class VakifPays3DModel
    {
        public string PostUrl { get; set; } = string.Empty;
        public Dictionary<string, string> PostData { get; set; } = new();
    }

    private readonly HttpClient _httpClient;

    private const string ApiUrlTest = "https://testpos.vakifpays.com.tr/vakifpays/api/v2";
    private const string ApiUrlLive = "https://pos.vakifpays.com.tr/vakifpays/api/v2";
    private const string Sale3DUrlTest = "https://testpos.vakifpays.com.tr/vakifpays/api/v2/post/sale3d/{0}";
    private const string Sale3DUrlLive = "https://pos.vakifpays.com.tr/vakifpays/api/v2/post/sale3d/{0}";

    private const string MerchantUser = "apitest48@vakifpays.com.tr";
    private const string MerchantPassword = "Api.123.1234";
    private const string Merchant = "10009011";

    public VakifPaysService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<VakifPays3DModel> Get3DSecureUrl(PaymentRequest model)
    {
        var pan = DigitsOnly(model.CardNumber);
        var cvv = DigitsOnly(model.Cvv);
        var month = NormalizeMonth(model.ExpiryMonth);
        var year = NormalizeYear(model.ExpiryYear);
        var paymentSystem = await ResolvePaymentSystemFromBin(pan, model.TestPlatform);
        if (string.IsNullOrWhiteSpace(paymentSystem))
            paymentSystem = "vakifbank";

        var token = await GetSessionToken(model);
        return new VakifPays3DModel
        {
            PostUrl = string.Format(model.TestPlatform ? Sale3DUrlTest : Sale3DUrlLive, token),
            PostData = new Dictionary<string, string>
            {
                { "points", "" },
                { "paymentSystem", paymentSystem },
                { "panname", model.CardOwner },
                { "cardOwner", model.CardOwner },
                { "cardName", model.CardOwner },
                { "pan", pan },
                { "expiryMonth", month },
                { "expiryYear", year },
                { "cvv", cvv },
                { "installmentCount", model.InstallmentCount.ToString() }
            }
        };
    }

    private async Task<string> ResolvePaymentSystemFromBin(string pan, bool testPlatform)
    {
        if (pan.Length < 6) return "";
        var bin = pan[..6];
        var result = await BinInstallmentQuery(bin, testPlatform);
        if (result.Raw == null || !result.Raw.TryGetValue("installmentPaymentSystem", out var ipsObj) || ipsObj == null) return "";

        var token = Newtonsoft.Json.Linq.JToken.FromObject(ipsObj);
        var candidates = new[]
        {
            token["paymentSystem"],
            token["paymentSystemType"],
            token["paymentSystemName"],
            token["name"],
            token["code"]
        };

        return candidates.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x?.ToString()))?.ToString() ?? "";
    }

    public async Task<SaleResponse> Sale(PaymentRequest request)
    {
        if (request.Use3D) return await Sale3D(request);

        var payload = BuildBasePayload(request);
        payload["ACTION"] = "SALE";
        payload["NAMEONCARD"] = request.CardOwner;
        payload["CARDPAN"] = request.CardNumber.Replace(" ", "");
        payload["CARDEXPIRY"] = $"{request.ExpiryMonth}.{request.ExpiryYear}";
        payload["CARDCVV"] = request.Cvv;
        payload["INSTALLMENTS"] = request.InstallmentCount.ToString();

        var dic = await Request(payload, request.TestPlatform);
        return MapSaleResponse(dic, request.OrderNumber);
    }

    public async Task<bool> Is3DSupportedByCard(string cardNumber, bool testPlatform = true)
    {
        var pan = DigitsOnly(cardNumber);
        if (pan.Length < 6) return true;

        var result = await BinInstallmentQuery(pan[..6], testPlatform);
        if (result.Raw == null || !result.Raw.TryGetValue("installmentPaymentSystem", out var ipsObj) || ipsObj == null)
            return true;

        var token = Newtonsoft.Json.Linq.JToken.FromObject(ipsObj);
        var candidates = new[]
        {
            token["supports3D"],
            token["supports3d"],
            token["is3DSupported"],
            token["secure3D"],
            token["use3D"]
        };

        foreach (var c in candidates)
        {
            var s = c?.ToString();
            if (string.IsNullOrWhiteSpace(s)) continue;
            if (bool.TryParse(s, out var b)) return b;
            if (s == "1") return true;
            if (s == "0") return false;
        }

        return true;
    }

    public async Task<CancelRefundResponse> Cancel(CancelRefundRequest request)
    {
        var payload = AuthPayload();
        payload["ACTION"] = "VOID";
        payload["PGTRANID"] = request.TransactionId;
        payload["REFLECTCOMMISSION"] = "No";

        var dic = await Request(payload, request.TestPlatform);
        return MapCancelRefund(dic);
    }

    public async Task<CancelRefundResponse> Refund(CancelRefundRequest request)
    {
        var payload = AuthPayload();
        payload["ACTION"] = "REFUND";
        payload["PGTRANID"] = request.TransactionId;
        payload["AMOUNT"] = ToAmount(request.Amount);
        payload["CURRENCY"] = request.Currency;
        payload["REFLECTCOMMISSION"] = "No";

        var dic = await Request(payload, request.TestPlatform);
        return MapCancelRefund(dic);
    }

    public async Task<InstallmentQueryResponse> BinInstallmentQuery(string bin, bool testPlatform = true)
    {
        var payload = AuthPayload();
        payload["ACTION"] = "QUERYPAYMENTSYSTEMS";
        payload["BIN"] = bin;

        var dic = await Request(payload, testPlatform);
        return new InstallmentQueryResponse { Raw = dic, Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00" };
    }

    public async Task<InstallmentQueryResponse> AllInstallmentQuery(bool testPlatform = true)
    {
        var payload = AuthPayload();
        payload["ACTION"] = "QUERYINSTALLMENT";
        payload["STATUS"] = "OK";

        var dic = await Request(payload, testPlatform);
        return new InstallmentQueryResponse { Raw = dic, Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00" };
    }

    public SaleResponse Sale3DResponse(IFormCollection form)
    {
        var success = form["responseCode"].ToString() == "00";
        return new SaleResponse
        {
            Success = success,
            Message = success ? form["responseMsg"].ToString() : $"{form["responseMsg"]} - {form["errorMsg"]}",
            OrderNumber = form["merchantPaymentId"].ToString(),
            TransactionId = form["pgTranId"].ToString()
        };
    }

    public SaleQueryResponse SaleQuery() => new() { Success = false, Message = "Bu sanal pos için satış sorgulama işlemi şuan desteklenmiyor" };
    public async Task<SaleQueryResponse> QueryTransaction(string merchantPaymentId, bool testPlatform = true)
    {
        var payload = AuthPayload();
        payload["ACTION"] = "QUERYTRANSACTION";
        payload["MERCHANTPAYMENTID"] = merchantPaymentId;
        var dic = await Request(payload, testPlatform);
        return new SaleQueryResponse
        {
            Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00",
            Message = dic.GetValueOrDefault("responseMsg")?.ToString() ?? "",
            Raw = dic
        };
    }

    private async Task<SaleResponse> Sale3D(PaymentRequest request)
    {
        var model = await Get3DSecureUrl(request);
        return new SaleResponse { Success = true, RedirectHtml = true, RedirectUrl = model.PostUrl };
    }

    private async Task<string> GetSessionToken(PaymentRequest model)
    {
        var payload = BuildBasePayload(model);
        payload["ACTION"] = "SESSIONTOKEN";
        payload["SESSIONTYPE"] = "PAYMENTSESSION";
        payload["ORDERITEMS"] = "[{\"code\":\"POSCEK\",\"name\":\"Cari Tahsilat\",\"description\":\"CariTahsilat\",\"quantity\":1,\"amount\":" + ToAmount(model.Amount) + "}]";

        var result = await Request(payload, model.TestPlatform);
        if (result.GetValueOrDefault("responseCode")?.ToString() == "00" && result.ContainsKey("sessionToken"))
            return result["sessionToken"]?.ToString() ?? "";

        throw new Exception($"VakıfPayS oturum hatası: {JsonConvert.SerializeObject(result)}");
    }

    private Dictionary<string, string> BuildBasePayload(PaymentRequest request)
    {
        var payload = AuthPayload();
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
        payload["AMOUNT"] = ToAmount(request.Amount);
        payload["CURRENCY"] = request.Currency;
        return payload;
    }

    private static string ToAmount(decimal amount) => amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")).Replace(".", "").Replace(",", ".");
    private static string DigitsOnly(string input) => new((input ?? string.Empty).Where(char.IsDigit).ToArray());
    private static string NormalizeMonth(string value)
    {
        if (!int.TryParse(DigitsOnly(value), out var month) || month is < 1 or > 12) return "01";
        return month.ToString("00");
    }

    private static string NormalizeYear(string value)
    {
        var digits = DigitsOnly(value);
        if (digits.Length == 2) return "20" + digits;
        if (digits.Length == 4) return digits;
        return DateTime.UtcNow.Year.ToString();
    }

    private static Dictionary<string, string> AuthPayload() => new()
    {
        { "MERCHANTUSER", MerchantUser },
        { "MERCHANTPASSWORD", MerchantPassword },
        { "MERCHANT", Merchant }
    };

    private async Task<Dictionary<string, object>> Request(Dictionary<string, string> payload, bool testPlatform)
    {
        using var content = new FormUrlEncodedContent(payload);
        using var response = await _httpClient.PostAsync(testPlatform ? ApiUrlTest : ApiUrlLive, content);
        var raw = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Dictionary<string, object>>(raw) ?? new Dictionary<string, object>();
    }

    private static SaleResponse MapSaleResponse(Dictionary<string, object> dic, string orderNumber) => new()
    {
        Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00",
        Message = dic.GetValueOrDefault("responseMsg")?.ToString() ?? "",
        OrderNumber = orderNumber,
        TransactionId = dic.GetValueOrDefault("pgTranId")?.ToString() ?? "",
        Raw = dic
    };

    private static CancelRefundResponse MapCancelRefund(Dictionary<string, object> dic) => new()
    {
        Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00",
        Message = dic.GetValueOrDefault("responseMsg")?.ToString() ?? "",
        Raw = dic
    };
}

public class PaymentRequest
{
    public bool TestPlatform { get; set; } = true;
    public bool Use3D { get; set; } = true;
    public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N");
    public string CardOwner { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int InstallmentCount { get; set; } = 1;
    public string Currency { get; set; } = "TRY";
    public string CustomerId { get; set; } = Guid.NewGuid().ToString();
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerIp { get; set; } = "127.0.0.1";
    public string CustomerPhone { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string BillToAddressLine { get; set; } = string.Empty;
    public string BillToCity { get; set; } = string.Empty;
    public string BillToCountry { get; set; } = "Turkey";
    public string BillToPostalCode { get; set; } = string.Empty;
    public string BillToPhone { get; set; } = string.Empty;
    public string ShipToAddressLine { get; set; } = string.Empty;
    public string ShipToCity { get; set; } = string.Empty;
    public string ShipToCountry { get; set; } = "Turkey";
    public string ShipToPostalCode { get; set; } = string.Empty;
    public string ShipToPhone { get; set; } = string.Empty;
}

public class SaleResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public bool RedirectHtml { get; set; }
    public string RedirectUrl { get; set; } = string.Empty;
    public Dictionary<string, object>? Raw { get; set; }
}

public class CancelRefundRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public bool TestPlatform { get; set; } = true;
}

public class CancelRefundResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Raw { get; set; }
}

public class InstallmentQueryResponse
{
    public bool Success { get; set; }
    public Dictionary<string, object>? Raw { get; set; }
}

public class SaleQueryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Raw { get; set; }
}

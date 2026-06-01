using Microsoft.Extensions.Logging;
using TriPay.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Models;
using TriPay.Services.Diagnostics;
using TriPay.Services.Providers.VakifPays.Helpers;
using TriPay.Services.Providers.VakifPays.Models;

namespace TriPay.Services.Providers.VakifPays;

/// <summary>
/// VakıfPayS kanalı — Iyzico/Vakıfbank ile aynı desen: <see cref="HttpPaymentGatewayBase"/> + appsettings.
/// </summary>
public sealed class VakifPaysGatewayProvider : HttpPaymentGatewayBase
{
    private const string ApiUrlTestDefault = "https://testpos.vakifpays.com.tr/vakifpays/api/v2";
    private const string ApiUrlLiveDefault = "https://pos.vakifpays.com.tr/vakifpays/api/v2";
    private const string Sale3DUrlTestTemplate = "https://testpos.vakifpays.com.tr/vakifpays/api/v2/post/sale3d/{0}";
    private const string Sale3DUrlLiveTemplate = "https://pos.vakifpays.com.tr/vakifpays/api/v2/post/sale3d/{0}";

    private string? _merchantUser;
    private string? _merchantPassword;
    private string? _merchantCode;
    private bool _isTestMode;

    /// <summary>Gateway ayarları ve HTTP istemcisi ile VakıfPayS provider oluşturur.</summary>
    public VakifPaysGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<VakifPaysGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override string GatewayName => PaymentGatewayNames.VakifPays;

    /// <inheritdoc />
    public override string DisplayName => "VakıfPayS";

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        if (!await InitializeVakifPaysSettingsAsync())
            return Result<PaymentGatewayInitializeResponseDto>.Failure("VakıfPayS ayarları yüklenemedi.");

        var supports3D = await Is3DSupportedByCardAsync(request.Payment.CardNumber, request.Payment.TestPlatform);
        if (!supports3D)
        {
            request.Payment.Use3D = false;
            var sale = await SaleAsync(request.Payment);
            return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
            {
                Success = sale.Success,
                Message = sale.Message,
                RedirectUrl = null,
                RedirectHtml = null
            });
        }

        var model = await Get3DSecureUrlAsync(request.Payment);
        PaymentDiagnostic.LogOutbound3DForm(
            GatewayName,
            model.PostUrl,
            model.PostData,
            "Tarayıcı otomatik POST (sale3d)");

        var html = VakifPaysAutoPostHtmlBuilder.Build(model.PostUrl, model.PostData);

        return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
        {
            Success = true,
            Message = "3D ödeme başlatıldı",
            RedirectHtml = html,
            RedirectUrl = model.PostUrl
        });
    }

    /// <inheritdoc />
    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

        var success = request.RawData.TryGetValue("responseCode", out var code) && code == "00";
        request.RawData.TryGetValue("responseMsg", out var msg);
        request.RawData.TryGetValue("merchantPaymentId", out var order);
        request.RawData.TryGetValue("pgTranId", out var tran);

        var response = new PaymentGatewayCallbackResponseDto
        {
            Success = success,
            Message = msg ?? string.Empty,
            OrderNumber = order ?? string.Empty,
            TransactionId = tran ?? string.Empty,
            ResponseCode = code ?? string.Empty,
            ErrorMessage = request.RawData.GetValueOrDefault("errorMsg") ?? string.Empty
        };

        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(response));
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
    {
        if (!await InitializeVakifPaysSettingsAsync())
            return Result<PaymentGatewayInstallmentResponseDto>.Failure("VakıfPayS ayarları yüklenemedi.");

        var rawCard = string.IsNullOrWhiteSpace(request.CardNumber) ? (request.BinNumber ?? "") : request.CardNumber;
        var digits = VakifPaysHttpHelper.DigitsOnly(rawCard);
        if (digits.Length < 6)
            return Result<PaymentGatewayInstallmentResponseDto>.Failure("Geçersiz kart numarası.");

        var result = await BinInstallmentQueryAsync(digits[..6], request.TestPlatform);
        var list = new List<InstallmentOptionDto>
        {
            new()
            {
                Count = 1,
                Rate = 0m,
                Total = request.Amount,
                Monthly = request.Amount,
                Label = $"1 (Tek Çekim) - {request.Amount:N2} TL"
            }
        };

        if (result.Raw != null && result.Raw.TryGetValue("installmentPaymentSystem", out var ipsObj) && ipsObj != null)
        {
            var root = JToken.FromObject(ipsObj);
            if (root["installmentList"] is JArray installments)
            {
                foreach (var item in installments)
                {
                    var count = item["count"]?.Value<int>() ?? 0;
                    if (count <= 1) continue;
                    var rate = item["customerCostCommissionRate"]?.Value<decimal?>()
                               ?? item["interestRate"]?.Value<decimal?>() ?? 0m;
                    var total = request.Amount * (1 + rate / 100m);
                    var monthly = total / count;
                    list.Add(new InstallmentOptionDto
                    {
                        Count = count,
                        Rate = rate,
                        Total = total,
                        Monthly = monthly,
                        Label =
                            $"{count} Taksit - %{rate:N2} vade farkı - Aylık {monthly:N2} TL (Toplam {total:N2} TL)"
                    });
                }
            }
        }

        return Result<PaymentGatewayInstallmentResponseDto>.Success(new PaymentGatewayInstallmentResponseDto
        {
            Success = true,
            Installments = list.GroupBy(x => x.Count).Select(x => x.First()).OrderBy(x => x.Count).ToList()
        });
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
    {
        if (!await InitializeVakifPaysSettingsAsync())
            return Result<PaymentGatewayStatusResponseDto>.Failure("VakıfPayS ayarları yüklenemedi.");

        var query = await QueryTransactionAsync(paymentId, _isTestMode);
        return Result<PaymentGatewayStatusResponseDto>.Success(new PaymentGatewayStatusResponseDto
        {
            Success = query.Success,
            Message = query.Message,
            ResponseCode = query.Raw?.GetValueOrDefault("responseCode")?.ToString() ?? string.Empty,
            Raw = query.Raw
        });
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
    {
        var callback = await ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto { RawData = request.RawData });
        if (!callback.IsSuccess || callback.Data == null)
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(callback.ErrorMessage ?? "3D callback işlenemedi.");

        return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
        {
            Success = callback.Data.Success,
            Message = callback.Data.Message,
            OrderNumber = callback.Data.OrderNumber,
            TransactionId = callback.Data.TransactionId,
            ResponseCode = callback.Data.ResponseCode,
            ErrorMessage = callback.Data.ErrorMessage
        });
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(
        string paymentId,
        decimal? amount = null)
    {
        if (!await InitializeVakifPaysSettingsAsync())
            return Result<PaymentGatewayRefundResponseDto>.Failure("VakıfPayS ayarları yüklenemedi.");

        var refund = await RefundAsync(new CancelRefundRequest
        {
            TransactionId = paymentId,
            Amount = amount ?? 0m,
            TestPlatform = _isTestMode
        });

        return Result<PaymentGatewayRefundResponseDto>.Success(new PaymentGatewayRefundResponseDto
        {
            Success = refund.Success,
            Message = refund.Message,
            Raw = refund.Raw
        });
    }

    /// <inheritdoc />
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode,
        string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        rawData.TryGetValue("responseCode", out var status);
        rawData.TryGetValue("pgTranId", out var paymentId);
        rawData.TryGetValue("merchantPaymentId", out var conversationId);
        rawData.TryGetValue("responseMsg", out var paymentStatus);
        rawData.TryGetValue("errorCode", out var errorCode);
        rawData.TryGetValue("errorMsg", out var errorMessage);
        return (status, paymentId, conversationId, paymentStatus, errorCode, errorMessage);
    }

    private async Task<bool> InitializeVakifPaysSettingsAsync()
    {
        var config = await GetGatewayConfigAsync();
        if (config == null)
            return false;

        _isTestMode = config.IsTestMode;
        _merchantUser = GetSetting(config, "MerchantUser", "ApiUser");
        _merchantPassword = GetSetting(config, "MerchantPassword");
        _merchantCode = GetSetting(config, "Merchant", "MerchantId");

        return !string.IsNullOrWhiteSpace(_merchantUser)
               && !string.IsNullOrWhiteSpace(_merchantPassword)
               && !string.IsNullOrWhiteSpace(_merchantCode);
    }

    private static string? GetSetting(GatewayConfig config, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (config.Settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private string ApiUrl(bool testPlatform)
        => testPlatform ? ApiUrlTestDefault : ApiUrlLiveDefault;

    private string Sale3DUrlTemplate(bool testPlatform)
        => testPlatform ? Sale3DUrlTestTemplate : Sale3DUrlLiveTemplate;

    private Dictionary<string, string> AuthPayload()
        => new()
        {
            ["MERCHANTUSER"] = _merchantUser!,
            ["MERCHANTPASSWORD"] = _merchantPassword!,
            ["MERCHANT"] = _merchantCode!
        };

    private async Task<Dictionary<string, object>> PostFormAsync(
        Dictionary<string, string> payload,
        bool testPlatform)
    {
        var raw = await MakeFormRequestAsync(ApiUrl(testPlatform), payload);
        return VakifPaysHttpHelper.ParseJsonDictionary(raw);
    }

    private async Task<VakifPays3DModel> Get3DSecureUrlAsync(PaymentRequest model)
    {
        var pan = VakifPaysHttpHelper.DigitsOnly(model.CardNumber);
        var paymentSystem = await ResolvePaymentSystemFromBinAsync(pan, model.TestPlatform);
        if (string.IsNullOrWhiteSpace(paymentSystem))
            paymentSystem = "vakifbank";

        var token = await GetSessionTokenAsync(model);
        return new VakifPays3DModel
        {
            PostUrl = string.Format(Sale3DUrlTemplate(model.TestPlatform), token),
            PostData = new Dictionary<string, string>
            {
                ["points"] = "",
                ["paymentSystem"] = paymentSystem,
                ["panname"] = model.CardOwner,
                ["cardOwner"] = model.CardOwner,
                ["cardName"] = model.CardOwner,
                ["pan"] = pan,
                ["expiryMonth"] = VakifPaysHttpHelper.NormalizeMonth(model.ExpiryMonth),
                ["expiryYear"] = VakifPaysHttpHelper.NormalizeYear(model.ExpiryYear),
                ["cvv"] = VakifPaysHttpHelper.DigitsOnly(model.Cvv),
                ["installmentCount"] = model.InstallmentCount.ToString()
            }
        };
    }

    private async Task<string> ResolvePaymentSystemFromBinAsync(string pan, bool testPlatform)
    {
        if (pan.Length < 6) return "";
        var result = await BinInstallmentQueryAsync(pan[..6], testPlatform);
        if (result.Raw == null
            || !result.Raw.TryGetValue("installmentPaymentSystem", out var ipsObj)
            || ipsObj == null)
            return "";

        var token = JToken.FromObject(ipsObj);
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

    private async Task<SaleResponse> SaleAsync(PaymentRequest request)
    {
        if (request.Use3D)
            return new SaleResponse { Success = true, RedirectHtml = true, RedirectUrl = (await Get3DSecureUrlAsync(request)).PostUrl };

        var payload = BuildBasePayload(request);
        payload["ACTION"] = "SALE";
        payload["NAMEONCARD"] = request.CardOwner;
        payload["CARDPAN"] = request.CardNumber.Replace(" ", "");
        payload["CARDEXPIRY"] = $"{request.ExpiryMonth}.{request.ExpiryYear}";
        payload["CARDCVV"] = request.Cvv;
        payload["INSTALLMENTS"] = request.InstallmentCount.ToString();

        var dic = await PostFormAsync(payload, request.TestPlatform);
        return MapSaleResponse(dic, request.OrderNumber);
    }

    private async Task<bool> Is3DSupportedByCardAsync(string cardNumber, bool testPlatform)
    {
        var pan = VakifPaysHttpHelper.DigitsOnly(cardNumber);
        if (pan.Length < 6) return true;

        var result = await BinInstallmentQueryAsync(pan[..6], testPlatform);
        if (result.Raw == null
            || !result.Raw.TryGetValue("installmentPaymentSystem", out var ipsObj)
            || ipsObj == null)
            return true;

        var token = JToken.FromObject(ipsObj);
        foreach (var c in new[] { token["supports3D"], token["supports3d"], token["is3DSupported"], token["secure3D"], token["use3D"] })
        {
            var s = c?.ToString();
            if (string.IsNullOrWhiteSpace(s)) continue;
            if (bool.TryParse(s, out var b)) return b;
            if (s == "1") return true;
            if (s == "0") return false;
        }

        return true;
    }

    private async Task<CancelRefundResponse> RefundAsync(CancelRefundRequest request)
    {
        var payload = AuthPayload();
        payload["ACTION"] = "REFUND";
        payload["PGTRANID"] = request.TransactionId;
        payload["AMOUNT"] = VakifPaysHttpHelper.ToAmount(request.Amount);
        payload["CURRENCY"] = request.Currency;
        payload["REFLECTCOMMISSION"] = "No";

        var dic = await PostFormAsync(payload, request.TestPlatform);
        return MapCancelRefund(dic);
    }

    private async Task<InstallmentQueryResponse> BinInstallmentQueryAsync(string bin, bool testPlatform)
    {
        var payload = AuthPayload();
        payload["ACTION"] = "QUERYPAYMENTSYSTEMS";
        payload["BIN"] = bin;

        var dic = await PostFormAsync(payload, testPlatform);
        return new InstallmentQueryResponse
        {
            Raw = dic,
            Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00"
        };
    }

    private async Task<SaleQueryResponse> QueryTransactionAsync(string merchantPaymentId, bool testPlatform)
    {
        var payload = AuthPayload();
        payload["ACTION"] = "QUERYTRANSACTION";
        payload["MERCHANTPAYMENTID"] = merchantPaymentId;
        var dic = await PostFormAsync(payload, testPlatform);
        return new SaleQueryResponse
        {
            Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00",
            Message = dic.GetValueOrDefault("responseMsg")?.ToString() ?? "",
            Raw = dic
        };
    }

    private async Task<string> GetSessionTokenAsync(PaymentRequest model)
    {
        var payload = BuildBasePayload(model);
        payload["ACTION"] = "SESSIONTOKEN";
        payload["SESSIONTYPE"] = "PAYMENTSESSION";
        payload["ORDERITEMS"] =
            "[{\"code\":\"POSCEK\",\"name\":\"Cari Tahsilat\",\"description\":\"CariTahsilat\",\"quantity\":1,\"amount\":" +
            VakifPaysHttpHelper.ToAmount(model.Amount) + "}]";

        var result = await PostFormAsync(payload, model.TestPlatform);
        if (result.GetValueOrDefault("responseCode")?.ToString() == "00" && result.ContainsKey("sessionToken"))
            return result["sessionToken"]?.ToString() ?? "";

        throw new InvalidOperationException($"VakıfPayS oturum hatası: {JsonConvert.SerializeObject(result)}");
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
        payload["AMOUNT"] = VakifPaysHttpHelper.ToAmount(request.Amount);
        payload["CURRENCY"] = request.Currency;
        return payload;
    }

    private static SaleResponse MapSaleResponse(Dictionary<string, object> dic, string orderNumber)
        => new()
        {
            Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00",
            Message = dic.GetValueOrDefault("responseMsg")?.ToString() ?? "",
            OrderNumber = orderNumber,
            TransactionId = dic.GetValueOrDefault("pgTranId")?.ToString() ?? "",
            Raw = dic
        };

    private static CancelRefundResponse MapCancelRefund(Dictionary<string, object> dic)
        => new()
        {
            Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00",
            Message = dic.GetValueOrDefault("responseMsg")?.ToString() ?? "",
            Raw = dic
        };
}

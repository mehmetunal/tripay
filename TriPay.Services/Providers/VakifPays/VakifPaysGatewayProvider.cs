using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;
using TriPay.Services.Providers.Protocols.ApiV2;
using TriPay.Services.Providers.VakifPays.Models;

namespace TriPay.Services.Providers.VakifPays;

/// <summary>
/// VakıfPayS kanalı — API v2 protokolü; 3D akışı tarayıcı otomatik POST ile çalışır.
/// </summary>
public sealed class VakifPaysGatewayProvider : ApiV2ProtocolGatewayBase
{
    /// <summary>Gateway ayarları ve HTTP istemcisi ile VakıfPayS provider oluşturur.</summary>
    public VakifPaysGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<VakifPaysGatewayProvider> logger)
        : base(ApiV2Endpoints.VakifPays, settingsProvider, httpClientFactory, logger)
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
        if (!await EnsureProtocolSettingsAsync())
            return Result<PaymentGatewayInitializeResponseDto>.Failure("VakıfPayS ayarları yüklenemedi.");

        var card = request.Payment;
        var supports3D = await Is3DSupportedByCardAsync(card.CardNumber, card.TestPlatform);
        if (!supports3D)
        {
            card.Use3D = false;
            var sale = await SaleAsync(card);
            return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
            {
                Success = sale.Success,
                Message = sale.Message,
                RedirectUrl = null,
                RedirectHtml = null
            });
        }

        var model = await BuildClientPost3DModelAsync(card);
        PaymentDiagnostic.LogOutbound3DForm(
            GatewayName,
            model.PostUrl,
            model.PostData,
            "Tarayıcı otomatik POST (sale3d)");

        var html = PaymentAutoPostHtmlBuilder.Build(model.PostUrl, model.PostData);

        return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
        {
            Success = true,
            Message = "3D ödeme başlatıldı",
            RedirectHtml = html,
            RedirectUrl = model.PostUrl
        });
    }

    private async Task<VakifPays3DModel> BuildClientPost3DModelAsync(PaymentRequest model)
    {
        var pan = PaymentCardHelper.DigitsOnly(model.CardNumber);
        var paymentSystem = await ResolvePaymentSystemFromBinAsync(pan, model.TestPlatform);
        if (string.IsNullOrWhiteSpace(paymentSystem))
            paymentSystem = "vakifbank";

        var token = await Api.CreateSessionTokenAsync(model);
        return new VakifPays3DModel
        {
            PostUrl = Api.BuildSale3DUrl(token, model.TestPlatform),
            PostData = new Dictionary<string, string>
            {
                ["points"] = "",
                ["paymentSystem"] = paymentSystem,
                ["panname"] = model.CardOwner,
                ["cardOwner"] = model.CardOwner,
                ["cardName"] = model.CardOwner,
                ["pan"] = pan,
                ["expiryMonth"] = PaymentCardHelper.NormalizeMonth(model.ExpiryMonth),
                ["expiryYear"] = PaymentCardHelper.NormalizeYear(model.ExpiryYear),
                ["cvv"] = PaymentCardHelper.DigitsOnly(model.Cvv),
                ["installmentCount"] = model.InstallmentCount.ToString()
            }
        };
    }

    private async Task<string> ResolvePaymentSystemFromBinAsync(string pan, bool testPlatform)
    {
        if (pan.Length < 6) return "";
        var result = await Api.QueryBinInstallmentsAsync(pan[..6], testPlatform);
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
        var payload = Api.CreateBasePayload(request);
        payload["ACTION"] = "SALE";
        payload["NAMEONCARD"] = request.CardOwner;
        payload["CARDPAN"] = PaymentCardHelper.DigitsOnly(request.CardNumber);
        payload["CARDEXPIRY"] = $"{request.ExpiryMonth}.{request.ExpiryYear}";
        payload["CARDCVV"] = request.Cvv;
        payload["INSTALLMENTS"] = request.InstallmentCount.ToString();

        var dic = await Api.PostFormAsync(payload, request.TestPlatform);
        return MapSaleResponse(dic, request.OrderNumber);
    }

    private async Task<bool> Is3DSupportedByCardAsync(string cardNumber, bool testPlatform)
    {
        var pan = PaymentCardHelper.DigitsOnly(cardNumber);
        if (pan.Length < 6) return true;

        var result = await Api.QueryBinInstallmentsAsync(pan[..6], testPlatform);
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

    private static SaleResponse MapSaleResponse(Dictionary<string, object> dic, string orderNumber)
        => new()
        {
            Success = dic.GetValueOrDefault("responseCode")?.ToString() == "00",
            Message = dic.GetValueOrDefault("responseMsg")?.ToString() ?? "",
            OrderNumber = orderNumber,
            TransactionId = dic.GetValueOrDefault("pgTranId")?.ToString() ?? "",
            Raw = dic
        };
}

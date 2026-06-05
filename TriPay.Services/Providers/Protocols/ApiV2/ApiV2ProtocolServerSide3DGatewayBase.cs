using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;

namespace TriPay.Services.Providers.Protocols.ApiV2;

/// <summary>
/// API v2 üzerinde sunucu tarafı sale3d POST ile 3D başlatan kanallar (Paratika, Payten MSU, ZiraatPay).
/// </summary>
public abstract class ApiV2ProtocolServerSide3DGatewayBase : ApiV2ProtocolGatewayBase
{
    /// <summary>Endpoint yapılandırması ile sunucu tarafı 3D taban sınıfını başlatır.</summary>
    protected ApiV2ProtocolServerSide3DGatewayBase(
        ApiV2EndpointConfig endpoints,
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger logger)
        : base(endpoints, settingsProvider, httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await EnsureProtocolSettingsAsync())
                return Result<PaymentGatewayInitializeResponseDto>.Failure($"{DisplayName} ayarları yüklenemedi.");

            var card = request.Payment;
            var sessionToken = await Api.CreateSessionTokenAsync(card);
            var sale3DUrl = Api.BuildSale3DUrl(sessionToken, card.TestPlatform);

            var formFields = new Dictionary<string, string>
            {
                ["points"] = "",
                ["paymentSystem"] = "",
                ["panname"] = card.CardOwner,
                ["cardOwner"] = card.CardOwner,
                ["pan"] = PaymentCardHelper.DigitsOnly(card.CardNumber),
                ["expiryMonth"] = PaymentCardHelper.NormalizeMonth(card.ExpiryMonth),
                ["expiryYear"] = PaymentCardHelper.NormalizeYear(card.ExpiryYear),
                ["cvv"] = PaymentCardHelper.DigitsOnly(card.Cvv),
                ["installmentCount"] = card.InstallmentCount.ToString()
            };

            PaymentDiagnostic.LogOutbound3DForm(GatewayName, sale3DUrl, formFields, "Sunucu tarafı sale3d POST");
            var htmlResponse = await MakeFormRequestAsync(sale3DUrl, formFields);

            if (string.IsNullOrWhiteSpace(htmlResponse))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("3D Secure yanıtı alınamadı.");

            var uri = new Uri(sale3DUrl);
            var baseUri = $"{uri.Scheme}://{uri.Host}";
            var html = $"<base href=\"{baseUri}\" /> {htmlResponse}";
            html = Api.AppendFraudMetrixScript(html, sessionToken);

            return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
            {
                Success = true,
                Message = "3D ödeme başlatıldı",
                RedirectHtml = html,
                RedirectUrl = sale3DUrl,
                PaymentId = card.OrderNumber,
                ConversationId = card.OrderNumber
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{Gateway} InitializePayment exception", GatewayName);
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }
}

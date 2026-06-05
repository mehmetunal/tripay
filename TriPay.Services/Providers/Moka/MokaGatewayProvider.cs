using System.Text.Json;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;
using TriPay.Services.Providers.Common;

namespace TriPay.Services.Providers.Moka;

/// <summary>Moka PaymentDealer REST sanal POS entegrasyonu.</summary>
public sealed class MokaGatewayProvider : HttpPaymentGatewayBase
{
    private const string ApiUrlTest = "https://service.refmoka.com";
    private const string ApiUrlLive = "https://service.moka.com";

    private string? _dealerCode;
    private string? _username;
    private string? _password;
    private bool _isTestMode;

    public MokaGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<MokaGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    public override string GatewayName => PaymentGatewayNames.Moka;
    public override string DisplayName => "Moka";

    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(
        PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Moka ayarları yüklenemedi.");

            var card = request.Payment;
            var checkKey = BankHashHelper.Sha256HexLower($"{_dealerCode}MK{_username}PD{_password}");
            var currency = card.Currency == "TRY" ? "TL" : card.Currency;

            var saleDic = new Dictionary<string, object>
            {
                ["CardHolderFullName"] = card.CardOwner,
                ["CardNumber"] = PaymentCardHelper.DigitsOnly(card.CardNumber),
                ["ExpMonth"] = PaymentCardHelper.NormalizeMonth(card.ExpiryMonth),
                ["ExpYear"] = PaymentCardHelper.NormalizeYear(card.ExpiryYear),
                ["CvcNumber"] = card.Cvv,
                ["Amount"] = Math.Round(card.Amount, 2),
                ["Currency"] = currency,
                ["InstallmentNumber"] = card.InstallmentCount > 1 ? card.InstallmentCount : 1,
                ["ClientIP"] = card.CustomerIp,
                ["OtherTrxCode"] = card.OrderNumber,
                ["IsPoolPayment"] = 0,
                ["IsTokenized"] = 0,
                ["Software"] = "tripay",
                ["IsPreAuth"] = 0,
                ["ReturnHash"] = 1,
                ["RedirectType"] = 0,
                ["RedirectUrl"] = card.ReturnUrl
            };

            var body = new Dictionary<string, object>
            {
                ["PaymentDealerAuthentication"] = new Dictionary<string, string>
                {
                    ["DealerCode"] = _dealerCode!,
                    ["Username"] = _username!,
                    ["Password"] = _password!,
                    ["CheckKey"] = checkKey
                },
                ["PaymentDealerRequest"] = saleDic
            };

            var url = $"{ApiBase()}/PaymentDealer/DoDirectPaymentThreeD";
            var raw = await MakeRequestAsyncRaw(url, HttpMethod.Post, JsonSerializer.Serialize(body));
            var dic = JsonResponseHelper.ParseDictionary(raw);

            if (dic.TryGetValue("ResultCode", out var rc) && rc?.ToString()?.Equals("Success", StringComparison.OrdinalIgnoreCase) == true
                && dic.TryGetValue("Data", out var data))
            {
                var dataDic = JsonSerializer.Deserialize<Dictionary<string, object?>>(data!.ToString()!,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (dataDic?.TryGetValue("Url", out var redirectUrl) == true && !string.IsNullOrWhiteSpace(redirectUrl?.ToString()))
                {
                    return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
                    {
                        Success = true,
                        Message = "3D ödeme başlatıldı",
                        RedirectUrl = redirectUrl.ToString(),
                        PaymentId = card.OrderNumber,
                        ConversationId = card.OrderNumber
                    });
                }
            }

            var message = dic.TryGetValue("ResultMessage", out var rm) ? rm?.ToString() : "3D başlatılamadı";
            return Result<PaymentGatewayInitializeResponseDto>.Failure(message ?? "3D başlatılamadı");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Moka InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(
        PaymentGatewayCallbackRequestDto request)
    {
        PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");
        request.RawData.TryGetValue("OtherTrxCode", out var order);
        request.RawData.TryGetValue("trxCode", out var trx);
        request.RawData.TryGetValue("resultCode", out var resultCode);
        request.RawData.TryGetValue("resultMessage", out var resultMessage);

        var success = string.IsNullOrWhiteSpace(resultCode) && string.IsNullOrWhiteSpace(resultMessage);
        return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
        {
            Success = success,
            Message = resultMessage ?? (success ? "3D doğrulama başarılı" : "3D doğrulama başarısız"),
            OrderNumber = order ?? string.Empty,
            TransactionId = trx ?? string.Empty,
            ErrorMessage = resultMessage ?? string.Empty
        }));
    }

    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(
        PaymentGatewayAuth3DSRequestDto request)
    {
        var callback = await ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto { RawData = request.RawData });
        if (!callback.IsSuccess || callback.Data == null)
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(callback.ErrorMessage ?? "3D callback işlenemedi.");

        if (!callback.Data.Success)
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure(callback.Data.ErrorMessage ?? "3D doğrulama başarısız.");

        return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
        {
            Success = true,
            Message = callback.Data.Message,
            OrderNumber = callback.Data.OrderNumber,
            TransactionId = callback.Data.TransactionId
        });
    }

    public override Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(Result<PaymentGatewayStatusResponseDto>.Failure("Moka durum sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(
        PaymentGatewayInstallmentRequestDto request)
        => Task.FromResult(Result<PaymentGatewayInstallmentResponseDto>.Failure("Moka taksit sorgusu desteklenmiyor."));

    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("Moka iade desteklenmiyor."));

    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode,
        string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        rawData.TryGetValue("trxCode", out var paymentId);
        rawData.TryGetValue("OtherTrxCode", out var conversationId);
        rawData.TryGetValue("resultCode", out var errorCode);
        rawData.TryGetValue("resultMessage", out var errorMessage);
        var success = string.IsNullOrWhiteSpace(errorCode);
        return (success ? "OK" : "FAIL", paymentId, conversationId, null, errorCode, errorMessage);
    }

    private async Task<bool> InitializeSettingsAsync()
    {
        var config = await GetGatewayConfigAsync();
        if (config == null) return false;

        _isTestMode = config.IsTestMode;
        _dealerCode = GatewaySettingsHelper.GetFirst(config, "MerchantId", "DealerCode");
        _username = GatewaySettingsHelper.GetFirst(config, "MerchantUser", "Username");
        _password = GatewaySettingsHelper.GetFirst(config, "MerchantPassword", "Password");

        return GatewaySettingsHelper.AllPresent(_dealerCode, _username, _password);
    }

    private string ApiBase() => _isTestMode ? ApiUrlTest : ApiUrlLive;

}

using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Interfaces;
using TriPay.Services.Models;
using TriPay.Services.Providers.Vakifbank.Helpers;
using TriPay.Core.Vakifbank;
using TriPay.Services.Diagnostics;
using TriPay.Services.Providers.Vakifbank.Models;

namespace TriPay.Services.Providers.Vakifbank;

/// <summary>
/// Vakıfbank MPI Enrollment + VPOS 3D Secure entegrasyonu.
/// İş mantığı Trimango <c>VakifbankGatewayProvider.cs</c> kaynağından TriPay DTO ve Redis depolamaya uyarlanmıştır.
/// </summary>
public sealed class VakifbankGatewayProvider : HttpPaymentGatewayBase
{
    private const string TransactionDeviceSource = "0";
    private readonly IVakifbankSaleStateStore _saleStateStore;
    private readonly IGatewayMetadataService _metadata;

    private string? _merchantId;
    private string? _merchantPassword;
    private string? _terminalNo;
    private string? _enrollmentUrl;
    private string? _verifyUrl;
    private bool _isTestMode;
    private List<int> _installmentCounts = new();
    private List<string> _binPrefixes = new();
    private string _resultCodeSuccess = "0000";
    private string _threeDsEnrolled = "Y";
    private string _threeDsAttempt = "A";
    private string _threeDsNotEnrolled = "N";
    private string _errorCodeIssuerException = "1001";
    private string _notEnrolledUserMessage = string.Empty;
    private IReadOnlyDictionary<string, string> _errorMap = new Dictionary<string, string>();

    private static readonly Dictionary<string, string> CurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TRY"] = "949",
        ["USD"] = "840",
        ["EUR"] = "978",
        ["GBP"] = "826"
    };

    private static readonly Dictionary<string, string> BrandCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["4"] = "100",
        ["5"] = "200",
        ["6"] = "200",
        ["3"] = "300"
    };

    /// <summary>Gateway ayarları, HTTP istemcisi ve satış durumu deposu ile Vakıfbank provider'ını oluşturur.</summary>
    public VakifbankGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        IVakifbankSaleStateStore saleStateStore,
        IGatewayMetadataService metadata,
        ILogger<VakifbankGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
        _saleStateStore = saleStateStore;
        _metadata = metadata;
    }

    /// <summary>Vakıfbank kanal kodu (<see cref="PaymentGatewayNames.Vakifbank"/>).</summary>
    public override string GatewayName => PaymentGatewayNames.Vakifbank;

    /// <summary>Kullanıcı arayüzünde görünen kanal adı.</summary>
    public override string DisplayName => "Vakıfbank";

    /// <summary>MPI enrollment isteği gönderir ve 3D auto-submit HTML üretir.</summary>
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Vakıfbank ayarları yüklenemedi.");

            var card = request.Payment;
            if (string.IsNullOrEmpty(card.CardNumber) || card.CardNumber.Length < 4)
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Geçersiz kart numarası.");

            var firstDigit = card.CardNumber.TrimStart()[..1];
            if (!BrandCodes.TryGetValue(firstDigit, out var brandCode))
                brandCode = VakifbankConstants.DefaultBrandCode;

            if (!CurrencyCodes.TryGetValue(card.Currency, out var currencyIso))
                currencyIso = VakifbankConstants.DefaultCurrencyCode;

            var purchaseAmount = card.Amount.ToString("0.00", CultureInfo.InvariantCulture);
            var expiryDate = BuildExpiryYYMM(card.ExpiryMonth, card.ExpiryYear);
            if (string.IsNullOrEmpty(expiryDate))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Geçersiz kart son kullanma tarihi.");

            var verifyEnrollmentRequestId = Guid.NewGuid().ToString("N");
            var sessionInfo = !string.IsNullOrWhiteSpace(card.OrderNumber) ? card.OrderNumber : verifyEnrollmentRequestId;
            var installment = card.InstallmentCount > 1 ? card.InstallmentCount.ToString() : string.Empty;

            var formParams = new Dictionary<string, string>
            {
                ["Pan"] = card.CardNumber.Trim().Replace(" ", ""),
                ["ExpiryDate"] = expiryDate,
                ["PurchaseAmount"] = purchaseAmount,
                ["Currency"] = currencyIso,
                ["BrandName"] = brandCode,
                ["VerifyEnrollmentRequestId"] = verifyEnrollmentRequestId,
                ["SessionInfo"] = sessionInfo,
                ["MerchantID"] = _merchantId!,
                ["MerchantPassword"] = _merchantPassword!,
                ["SuccessUrl"] = card.ReturnUrl,
                ["FailureUrl"] = card.ReturnUrl
            };
            if (!string.IsNullOrEmpty(installment))
                formParams["InstallmentCount"] = installment;

            var formBody = string.Join("&", formParams.Select(kv =>
                $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value ?? "")}"));

            var responseContent = await MakeRequestAsyncRaw(
                _enrollmentUrl!, HttpMethod.Post, formBody, null, "application/x-www-form-urlencoded");

            if (string.IsNullOrWhiteSpace(responseContent))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("3D Secure başlatılamadı.");

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseContent);

            var status = xmlDoc.SelectSingleNode("IPaySecure/Message/VERes/Status")?.InnerText?.Trim();
            if (!Is3DSuccessStatus(status))
            {
                var errorMsg = xmlDoc.SelectSingleNode("IPaySecure/ErrorMessage")?.InnerText?.Trim() ?? "3D Secure başlatılamadı.";
                var errorCode = xmlDoc.SelectSingleNode("IPaySecure/MessageErrorCode")?.InnerText?.Trim();
                if (string.Equals(status, _threeDsNotEnrolled, StringComparison.Ordinal)
                    && string.Equals(errorCode, _errorCodeIssuerException, StringComparison.Ordinal))
                {
                    errorMsg = _notEnrolledUserMessage;
                }

                Logger.LogWarning("Vakıfbank Enrollment failed: {Status}, {Code}, {Message}", status, errorCode, errorMsg);
                return Result<PaymentGatewayInitializeResponseDto>.Failure(errorMsg);
            }

            var paReq = xmlDoc.SelectSingleNode("IPaySecure/Message/VERes/PaReq")?.InnerText ?? "";
            var termUrl = xmlDoc.SelectSingleNode("IPaySecure/Message/VERes/TermUrl")?.InnerText ?? "";
            var md = xmlDoc.SelectSingleNode("IPaySecure/Message/VERes/MD")?.InnerText ?? "";
            var acsUrl = xmlDoc.SelectSingleNode("IPaySecure/Message/VERes/ACSUrl")?.InnerText ?? "";

            if (string.IsNullOrEmpty(acsUrl))
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Vakıfbank ACS URL alınamadı.");

            PaymentDiagnostic.LogOutbound3DForm(
                GatewayName,
                acsUrl,
                new Dictionary<string, string>
                {
                    ["PaReq"] = paReq,
                    ["TermUrl"] = termUrl,
                    ["MD"] = md
                },
                "MPI Enrollment sonrası ACS otomatik POST");

            var orderCode = sessionInfo;
            await _saleStateStore.SetAsync(orderCode, new VakifbankSaleState
            {
                OrderCode = orderCode,
                Cvv = card.Cvv,
                ClientIp = ResolveClientIp(card.CustomerIp),
                ExpiryYYYYMM = BuildExpiryYYYYMM(card.ExpiryMonth, card.ExpiryYear) ?? "",
                PurchaseAmount = purchaseAmount,
                CurrencyCode = currencyIso
            });

            return Result<PaymentGatewayInitializeResponseDto>.Success(new PaymentGatewayInitializeResponseDto
            {
                Success = true,
                PaymentId = verifyEnrollmentRequestId,
                ConversationId = orderCode,
                Message = "3D ödeme başlatıldı",
                RedirectHtml = BuildAutoSubmitForm(acsUrl, paReq, termUrl, md)
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vakıfbank InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    /// <summary>Vakıfbank 3D callback POST alanlarını işler ve PENDING durumu döndürür.</summary>
    public override async Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayCallbackResponseDto>.Failure("Vakıfbank ayarları yüklenemedi.");

            if (request.RawData.Count == 0)
                return Result<PaymentGatewayCallbackResponseDto>.Failure("Vakıfbank callback verisi boş.");

            PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "ProcessCallbackAsync");

            var status = GetRawValue(request.RawData, "Status");
            var verifyEnrollmentRequestId = GetRawValue(request.RawData, "VerifyEnrollmentRequestId");
            var sessionInfo = GetRawValue(request.RawData, "SessionInfo");
            var orderCode = !string.IsNullOrWhiteSpace(sessionInfo) ? sessionInfo : verifyEnrollmentRequestId;

            if (!Is3DSuccessStatus(status))
            {
                var errorCode = GetRawValue(request.RawData, "ErrorCode");
                var errorMessage = await ResolveErrorMessageAsync(errorCode, "3D doğrulama başarısız.");
                return Result<PaymentGatewayCallbackResponseDto>.Failure(errorMessage);
            }

            decimal? paidAmount = null;
            var purchAmount = GetRawValue(request.RawData, "PurchAmount");
            if (decimal.TryParse(purchAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amt))
                paidAmount = amt;

            return Result<PaymentGatewayCallbackResponseDto>.Success(new PaymentGatewayCallbackResponseDto
            {
                Success = true,
                Message = "3D doğrulama başarılı",
                PaymentId = verifyEnrollmentRequestId,
                OrderNumber = orderCode,
                PaymentStatus = "PENDING",
                PaidAmount = paidAmount,
                Currency = MapCurrencyCode(GetRawValue(request.RawData, "PurchCurrency"))
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vakıfbank ProcessCallback exception");
            return Result<PaymentGatewayCallbackResponseDto>.Failure($"Callback işlenirken hata: {ex.Message}");
        }
    }

    /// <summary>Redis'teki satış durumu ve VPOS XML ile 3D sonrası tahsilatı tamamlar.</summary>
    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Vakıfbank ayarları yüklenemedi.");

            if (request.RawData.Count == 0)
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Vakıfbank Auth3DS için callback verisi gerekli.");

            PaymentDiagnostic.LogInboundCallback(GatewayName, request.RawData, "Auth3DSAsync (VPOS öncesi)");

            var status = GetRawValue(request.RawData, "Status");
            if (!Is3DSuccessStatus(status))
            {
                var errorCode = GetRawValue(request.RawData, "ErrorCode");
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                    await ResolveErrorMessageAsync(errorCode, "3D doğrulama başarısız."));
            }

            var verifyEnrollmentRequestId = GetRawValue(request.RawData, "VerifyEnrollmentRequestId");
            var sessionInfo = GetRawValue(request.RawData, "SessionInfo");
            var orderCode = !string.IsNullOrWhiteSpace(request.ConversationId)
                ? request.ConversationId
                : (!string.IsNullOrWhiteSpace(sessionInfo) ? sessionInfo : verifyEnrollmentRequestId);

            var saleState = await _saleStateStore.GetAsync(orderCode!);
            if (saleState == null)
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Ödeme oturum verisi bulunamadı. Lütfen ödemeyi yeniden başlatın.");

            var pan = GetRawValue(request.RawData, "Pan");
            var expiry = GetRawValue(request.RawData, "Expiry");
            if (string.IsNullOrEmpty(expiry) && !string.IsNullOrEmpty(saleState.ExpiryYYYYMM))
                expiry = saleState.ExpiryYYYYMM;

            var currencyAmount = !string.IsNullOrWhiteSpace(saleState.PurchaseAmount)
                ? saleState.PurchaseAmount
                : GetRawValue(request.RawData, "PurchAmount");
            if (string.IsNullOrWhiteSpace(currencyAmount))
                currencyAmount = "0.00";
            else if (decimal.TryParse(currencyAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amt))
                currencyAmount = amt.ToString("0.00", CultureInfo.InvariantCulture);

            var currencyCode = !string.IsNullOrWhiteSpace(saleState.CurrencyCode)
                ? saleState.CurrencyCode
                : GetRawValue(request.RawData, "PurchCurrency");
            if (string.IsNullOrWhiteSpace(currencyCode))
                currencyCode = VakifbankConstants.DefaultCurrencyCode;

            var installmentCountStr = GetRawValue(request.RawData, "InstallmentCount");
            var installmentCount = 1;
            if (!string.IsNullOrWhiteSpace(installmentCountStr) && int.TryParse(installmentCountStr, out var inst))
                installmentCount = inst > 0 ? inst : 1;

            var cardHoldersName = GetRawValue(request.RawData, "card_holders_name");
            if (string.IsNullOrWhiteSpace(cardHoldersName))
                cardHoldersName = "CardHolder";

            var vposXml = BuildVposSaleXml(
                pan,
                NormalizeExpiryForVpos(expiry),
                currencyAmount,
                currencyCode,
                installmentCount,
                cardHoldersName,
                saleState.Cvv,
                GetRawValue(request.RawData, "Eci"),
                GetRawValue(request.RawData, "Cavv"),
                verifyEnrollmentRequestId,
                ResolveClientIp(saleState.ClientIp));

            var formBody = $"prmstr={WebUtility.UrlEncode(vposXml)}";
            var responseContent = await MakeRequestAsyncRaw(
                _verifyUrl!, HttpMethod.Post, formBody, null, "application/x-www-form-urlencoded");

            if (string.IsNullOrWhiteSpace(responseContent))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Ödeme tamamlanamadı.");

            var responseDoc = new XmlDocument();
            responseDoc.LoadXml(responseContent);
            var vpos = VakifbankXmlHelper.ParseVposResponse(responseDoc);

            if (!vpos.IsSuccessWithCode(_resultCodeSuccess))
            {
                var errMsg = !string.IsNullOrEmpty(vpos.ResultDetail)
                    ? vpos.ResultDetail
                    : await ResolveErrorMessageAsync(vpos.ResultCode, "Ödeme tamamlanamadı.");
                Logger.LogWarning("Vakıfbank Vpos Sale failed: {Code}, {Detail}", vpos.ResultCode, vpos.ResultDetail);
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(errMsg);
            }

            await _saleStateStore.RemoveAsync(orderCode!);

            decimal? paidAmount = decimal.TryParse(currencyAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var paid)
                ? paid
                : null;

            return Result<PaymentGatewayAuth3DSResponseDto>.Success(new PaymentGatewayAuth3DSResponseDto
            {
                Success = true,
                Status = "success",
                Message = "Ödeme tamamlandı",
                PaymentId = !string.IsNullOrEmpty(vpos.TransactionId) ? vpos.TransactionId : verifyEnrollmentRequestId,
                PaymentStatus = "SUCCESS",
                OrderNumber = orderCode ?? string.Empty,
                ConversationId = orderCode,
                PaidAmount = paidAmount,
                Price = paidAmount,
                Currency = MapCurrencyCode(currencyCode),
                Installment = installmentCount
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vakıfbank Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"Auth3DS işlemi sırasında hata: {ex.Message}");
        }
    }

    /// <summary>Vakıfbank için özet durum yanıtı döndürür (MVP).</summary>
    public override async Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayStatusResponseDto>.Failure("Vakıfbank ayarları yüklenemedi.");

            return Result<PaymentGatewayStatusResponseDto>.Success(new PaymentGatewayStatusResponseDto
            {
                Success = true,
                PaymentId = paymentId,
                Status = "success",
                PaymentStatus = "SUCCESS",
                Message = "Vakıfbank durum sorgusu özet yanıt döner.",
                ResponseCode = "SUCCESS",
                Currency = "TRY"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vakıfbank GetPaymentStatus exception");
            return Result<PaymentGatewayStatusResponseDto>.Failure($"Durum sorgusu hatası: {ex.Message}");
        }
    }

    /// <summary>Config'teki taksit sayıları ve BIN listesine göre yerel taksit seçenekleri üretir.</summary>
    public override async Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayInstallmentResponseDto>.Failure("Vakıfbank ayarları yüklenemedi.");

            if (request.Amount <= 0)
                return Result<PaymentGatewayInstallmentResponseDto>.Failure("Geçerli bir tutar giriniz.");

            var pan = (request.CardNumber ?? request.BinNumber ?? "").Trim().Replace(" ", "");
            if (pan.Length < 6)
                return Result<PaymentGatewayInstallmentResponseDto>.Failure(
                    "Taksit seçenekleri için kart numarası veya BIN (ilk 6 hane) gereklidir.");

            var bin = pan[..6];
            var isVakifbankCard = _binPrefixes.Any(prefix =>
                prefix.Length >= 6 && bin.StartsWith(prefix[..6], StringComparison.Ordinal));

            var options = new List<InstallmentOptionDto>();
            if (isVakifbankCard)
            {
                var counts = new HashSet<int>(_installmentCounts) { 1 };
                foreach (var n in counts.OrderBy(x => x))
                {
                    if (n < 1) continue;
                    var monthly = Math.Round(request.Amount / n, 2, MidpointRounding.AwayFromZero);
                    options.Add(new InstallmentOptionDto
                    {
                        Count = n,
                        Total = Math.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
                        Monthly = monthly,
                        Label = $"{n} Taksit"
                    });
                }
            }
            else
            {
                options.Add(new InstallmentOptionDto
                {
                    Count = 1,
                    Total = Math.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
                    Monthly = Math.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
                    Label = "Tek Çekim"
                });
            }

            return Result<PaymentGatewayInstallmentResponseDto>.Success(new PaymentGatewayInstallmentResponseDto
            {
                Success = true,
                Installments = options
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vakıfbank GetInstallmentInfo exception");
            return Result<PaymentGatewayInstallmentResponseDto>.Failure($"Taksit sorgusu hatası: {ex.Message}");
        }
    }

    /// <summary>VPOS REFUND XML isteği ile iade işlemini gerçekleştirir.</summary>
    public override async Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
    {
        try
        {
            if (!await InitializeSettingsAsync())
                return Result<PaymentGatewayRefundResponseDto>.Failure("Vakıfbank ayarları yüklenemedi.");

            var refundXml = $@"<VposRequest>
<MerchantId>{EscapeXml(_merchantId)}</MerchantId>
<Password>{EscapeXml(_merchantPassword)}</Password>
<TransactionType>Refund</TransactionType>
<ReferenceTransactionId>{EscapeXml(paymentId)}</ReferenceTransactionId>
<ClientIp>{EscapeXml(ResolveClientIp())}</ClientIp>
</VposRequest>";

            var formBody = $"prmstr={WebUtility.UrlEncode(refundXml)}";
            var responseContent = await MakeRequestAsyncRaw(
                _verifyUrl!, HttpMethod.Post, formBody, null, "application/x-www-form-urlencoded");

            if (string.IsNullOrWhiteSpace(responseContent))
                return Result<PaymentGatewayRefundResponseDto>.Failure("İade yanıtı alınamadı.");

            var responseDoc = new XmlDocument();
            responseDoc.LoadXml(responseContent);
            var vpos = VakifbankXmlHelper.ParseVposResponse(responseDoc);

            if (!vpos.IsSuccessWithCode(_resultCodeSuccess))
            {
                var errMsg = !string.IsNullOrEmpty(vpos.ResultDetail)
                    ? vpos.ResultDetail
                    : await ResolveErrorMessageAsync(vpos.ResultCode, "İade başarısız.");
                return Result<PaymentGatewayRefundResponseDto>.Failure(errMsg);
            }

            var raw = new Dictionary<string, object> { ["transactionId"] = vpos.TransactionId ?? paymentId };
            if (amount.HasValue)
                raw["refundAmount"] = amount.Value;

            return Result<PaymentGatewayRefundResponseDto>.Success(new PaymentGatewayRefundResponseDto
            {
                Success = true,
                Message = "İade tamamlandı",
                Raw = raw
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vakıfbank RefundPayment exception");
            return Result<PaymentGatewayRefundResponseDto>.Failure($"İade işlemi sırasında hata: {ex.Message}");
        }
    }

    /// <summary>Vakıfbank MPI callback alanlarını success/failure ve hata açıklamasına normalize eder.</summary>
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        if (rawData.Count == 0)
            return (null, null, null, null, null, null);

        var statusRaw = GetRawValue(rawData, "Status");
        var status = string.IsNullOrEmpty(statusRaw)
            ? null
            : (Is3DSuccessStatus(statusRaw) ? "success" : "failure");

        var paymentId = GetRawValue(rawData, "VerifyEnrollmentRequestId");
        var conversationId = GetRawValue(rawData, "SessionInfo");
        if (string.IsNullOrWhiteSpace(conversationId))
            conversationId = paymentId;

        var paymentStatus = status == "success" ? "SUCCESS" : "FAILURE";
        var errorCode = GetRawValue(rawData, "ErrorCode");
        var errorMessage = GetRawValue(rawData, "ErrorMessage");
        if (string.IsNullOrWhiteSpace(errorMessage) && !string.IsNullOrWhiteSpace(errorCode)
            && _errorMap.TryGetValue(errorCode, out var mapped))
            errorMessage = mapped;

        return (status, paymentId, conversationId, paymentStatus, errorCode, errorMessage);
    }

    private async Task<bool> InitializeSettingsAsync()
    {
        try
        {
            var config = await GetGatewayConfigAsync();
            if (config == null)
            {
                Logger.LogError("Vakıfbank ayarları bulunamadı.");
                return false;
            }

            var settings = config.Settings;
            if (!settings.TryGetValue("MerchantId", out _merchantId)
                || !settings.TryGetValue("MerchantPassword", out _merchantPassword)
                || !settings.TryGetValue("TerminalNo", out _terminalNo)
                || string.IsNullOrWhiteSpace(_merchantId)
                || string.IsNullOrWhiteSpace(_merchantPassword)
                || string.IsNullOrWhiteSpace(_terminalNo))
            {
                Logger.LogError("Vakıfbank MerchantId, MerchantPassword ve TerminalNo gerekli.");
                return false;
            }

            _isTestMode = config.IsTestMode;
            _enrollmentUrl = RequireSetting(settings, GatewaySettingKeys.EnrollmentUrl);
            _verifyUrl = RequireSetting(settings, GatewaySettingKeys.VerifyUrl);
            if (string.IsNullOrWhiteSpace(_enrollmentUrl) || string.IsNullOrWhiteSpace(_verifyUrl))
            {
                Logger.LogError("Vakıfbank EnrollmentUrl/VerifyUrl veritabanında tanımlı olmalıdır (GatewaySettings).");
                return false;
            }

            _resultCodeSuccess = settings.GetValueOrDefault(GatewaySettingKeys.ResultCodeSuccess) ?? "0000";
            _threeDsEnrolled = settings.GetValueOrDefault(GatewaySettingKeys.ThreeDsStatusEnrolled) ?? "Y";
            _threeDsAttempt = settings.GetValueOrDefault(GatewaySettingKeys.ThreeDsStatusAttempt) ?? "A";
            _threeDsNotEnrolled = settings.GetValueOrDefault(GatewaySettingKeys.ThreeDsStatusNotEnrolled) ?? "N";
            _errorCodeIssuerException = settings.GetValueOrDefault(GatewaySettingKeys.ErrorCodeIssuerException) ?? "1001";
            _notEnrolledUserMessage = settings.GetValueOrDefault(GatewaySettingKeys.NotEnrolledUserMessage)
                ?? "Kartınız 3D Secure ile doğrulanamadı.";

            _errorMap = await _metadata.GetErrorMapAsync(GatewayName);
            _installmentCounts = ResolveInstallmentCounts(settings);
            _binPrefixes = ResolveBinPrefixes(settings);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vakıfbank ayarları yüklenirken hata");
            return false;
        }
    }

    private string BuildVposSaleXml(
        string pan, string expiry, string currencyAmount, string currencyCode, int installmentCount,
        string cardHoldersName, string cvv, string eci, string cavv, string mpiTransactionId, string clientIp)
    {
        var xml = new StringBuilder();
        xml.Append(@"<?xml version=""1.0"" encoding=""utf-8""?><VposRequest>");
        xml.Append($"<MerchantId>{EscapeXml(_merchantId)}</MerchantId>");
        xml.Append($"<Password>{EscapeXml(_merchantPassword)}</Password>");
        xml.Append($"<TerminalNo>{EscapeXml(_terminalNo)}</TerminalNo>");
        xml.Append($"<Pan>{EscapeXml(pan)}</Pan>");
        xml.Append($"<Expiry>{EscapeXml(expiry)}</Expiry>");
        xml.Append($"<CurrencyAmount>{EscapeXml(currencyAmount)}</CurrencyAmount>");
        xml.Append($"<CurrencyCode>{EscapeXml(currencyCode)}</CurrencyCode>");
        xml.Append("<TransactionId></TransactionId><TransactionType>Sale</TransactionType>");
        if (installmentCount > 1)
            xml.Append($"<NumberOfInstallments>{installmentCount}</NumberOfInstallments>");
        xml.Append($"<CardHoldersName>{EscapeXml(cardHoldersName)}</CardHoldersName>");
        xml.Append($"<Cvv>{EscapeXml(cvv)}</Cvv>");
        xml.Append($"<ECI>{EscapeXml(eci)}</ECI>");
        xml.Append($"<CAVV>{EscapeXml(cavv)}</CAVV>");
        xml.Append($"<MpiTransactionId>{EscapeXml(mpiTransactionId)}</MpiTransactionId>");
        xml.Append($"<OrderId>{EscapeXml(mpiTransactionId)}</OrderId>");
        xml.Append($"<ClientIp>{EscapeXml(clientIp)}</ClientIp>");
        xml.Append($"<TransactionDeviceSource>{TransactionDeviceSource}</TransactionDeviceSource></VposRequest>");
        return xml.ToString();
    }

    private static string? RequireSetting(IReadOnlyDictionary<string, string> settings, string key)
        => settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;

    private async Task<string> ResolveErrorMessageAsync(string? errorCode, string fallback)
        => await _metadata.GetErrorMessageAsync(GatewayName, errorCode) ?? fallback;

    private static List<int> ResolveInstallmentCounts(IReadOnlyDictionary<string, string> settings)
    {
        if (!settings.TryGetValue("InstallmentCounts", out var configured) || string.IsNullOrWhiteSpace(configured))
            return new List<int>();

        return configured.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var n) ? n : 0)
            .Where(n => n >= 2 && n <= 12)
            .Distinct()
            .ToList();
    }

    private static List<string> ResolveBinPrefixes(IReadOnlyDictionary<string, string> settings)
    {
        if (!settings.TryGetValue("BinPrefixes", out var configured) || string.IsNullOrWhiteSpace(configured))
            return new List<string>();

        return configured.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    private bool Is3DSuccessStatus(string? status)
        => !string.IsNullOrWhiteSpace(status)
           && (status.Equals(_threeDsEnrolled, StringComparison.OrdinalIgnoreCase)
               || status.Equals(_threeDsAttempt, StringComparison.OrdinalIgnoreCase));

    private static string BuildAutoSubmitForm(string acsUrl, string paReq, string termUrl, string md)
        => $@"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""/><title>Yönlendiriliyor...</title></head>
<body>
<form name=""frmMpiForm"" id=""frmMpiForm"" method=""post"" action=""{WebUtility.HtmlEncode(acsUrl)}"">
<input type=""hidden"" name=""PaReq"" value=""{WebUtility.HtmlEncode(paReq)}""/>
<input type=""hidden"" name=""TermUrl"" value=""{WebUtility.HtmlEncode(termUrl)}""/>
<input type=""hidden"" name=""MD"" value=""{WebUtility.HtmlEncode(md)}""/>
</form>
<script type=""text/javascript"">document.forms[""frmMpiForm""].submit();</script>
<p>Yönlendiriliyorsunuz...</p>
</body>
</html>";

    private static string GetRawValue(IReadOnlyDictionary<string, string> raw, string key)
    {
        if (raw.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v))
            return v.Trim();
        return raw.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value?.Trim() ?? string.Empty;
    }

    private static string ResolveClientIp(string? preferred = null)
    {
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            var first = preferred.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(first) && System.Net.IPAddress.TryParse(first, out _))
                return first;
        }

        return "127.0.0.1";
    }

    private static string? MapCurrencyCode(string? code) => code switch
    {
        "949" => "TRY",
        "840" => "USD",
        "978" => "EUR",
        "826" => "GBP",
        _ => "TRY"
    };

    private static string BuildExpiryYYMM(string? month, string? year)
    {
        if (string.IsNullOrWhiteSpace(month) || string.IsNullOrWhiteSpace(year)) return string.Empty;
        var mm = new string(month.Where(char.IsDigit).ToArray()).PadLeft(2, '0');
        if (mm.Length > 2) mm = mm[^2..];
        var yy = new string(year.Where(char.IsDigit).ToArray());
        if (yy.Length > 2) yy = yy[^2..];
        if (yy.Length < 2 || !int.TryParse(mm, out var m) || m < 1 || m > 12) return string.Empty;
        return yy + mm;
    }

    private static string? BuildExpiryYYYYMM(string? month, string? year)
    {
        if (string.IsNullOrWhiteSpace(month) || string.IsNullOrWhiteSpace(year)) return null;
        var mm = new string(month.Where(char.IsDigit).ToArray()).PadLeft(2, '0');
        if (mm.Length > 2) mm = mm[^2..];
        var yy = new string(year.Where(char.IsDigit).ToArray());
        if (yy.Length == 2) yy = "20" + yy;
        else if (yy.Length > 4) yy = yy[^4..];
        if (yy.Length != 4 || !int.TryParse(mm, out var m) || m < 1 || m > 12) return null;
        return yy + mm;
    }

    private static string NormalizeExpiryForVpos(string? expiry)
    {
        if (string.IsNullOrWhiteSpace(expiry)) return string.Empty;
        var digits = new string(expiry.Where(char.IsDigit).ToArray());
        return digits.Length switch
        {
            4 => "20" + digits,
            6 => digits,
            _ => digits
        };
    }

    private static string EscapeXml(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}

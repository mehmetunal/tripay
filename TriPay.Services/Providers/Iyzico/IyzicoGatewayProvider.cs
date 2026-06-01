using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Models;
using TriPay.Services.Providers.Iyzico.Helpers;
using TriPay.Services.Providers.Iyzico.Models;

namespace TriPay.Services.Providers.Iyzico;

/// <summary>
/// Iyzico sanal POS entegrasyonu. İş mantığı Trimango
/// <c>IyzicoGatewayProvider.cs</c> kaynağından TriPay DTO/config yapısına uyarlanmıştır.
/// </summary>
public sealed class IyzicoGatewayProvider : HttpPaymentGatewayBase
{
    private const string SandboxBaseUrl = "https://sandbox-api.iyzipay.com";
    private const string ProductionBaseUrl = "https://api.iyzipay.com";
    private const string AuthorizationHeader = "Authorization";

    private string? _apiKey;
    private string? _secretKey;
    private bool _isTestMode;

    /// <summary>Gateway ayarları ve HTTP istemcisi ile Iyzico provider oluşturur.</summary>
    public IyzicoGatewayProvider(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<IyzicoGatewayProvider> logger)
        : base(settingsProvider, httpClientFactory, logger)
    {
    }

    /// <summary>Iyzico kanal kodu (<see cref="PaymentGatewayNames.Iyzico"/>).</summary>
    public override string GatewayName => PaymentGatewayNames.Iyzico;

    /// <summary>Kullanıcı arayüzünde görünen kanal adı.</summary>
    public override string DisplayName => "Iyzico";

    /// <summary>Iyzico 3D Secure initialize API çağrısını yapar ve HTML/URL döndürür.</summary>
    public override async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request)
    {
        try
        {
            if (!await InitializeIyzicoSettingsAsync())
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Iyzico ayarları yüklenemedi.");

            var p = request.Payment;
            var buyerFullName = string.Join(' ', new[] { p.CustomerName, p.CardOwner }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (string.IsNullOrWhiteSpace(buyerFullName))
                buyerFullName = p.ShipToAddressLine;
            var (buyerName, buyerSurname) = IyzicoStringHelper.SplitFullName(buyerFullName);

            var requestBody = new
            {
                locale = "tr",
                conversationId = p.OrderNumber,
                price = p.Amount.ToString("F2", CultureInfo.InvariantCulture),
                paidPrice = p.Amount.ToString("F2", CultureInfo.InvariantCulture),
                currency = p.Currency,
                installment = p.InstallmentCount,
                paymentChannel = "WEB",
                basketId = p.OrderNumber,
                paymentGroup = "PRODUCT",
                paymentCard = new
                {
                    cardNumber = p.CardNumber,
                    expireMonth = p.ExpiryMonth,
                    expireYear = NormalizeExpireYear(p.ExpiryYear),
                    cvc = p.Cvv,
                    cardHolderName = p.CardOwner,
                    registerCard = 0
                },
                buyer = new
                {
                    id = p.CustomerId,
                    name = buyerName,
                    surname = buyerSurname,
                    gsmNumber = p.CustomerPhone,
                    email = p.CustomerEmail,
                    identityNumber = "11111111111",
                    registrationAddress = p.BillToAddressLine,
                    ip = p.CustomerIp,
                    city = p.BillToCity,
                    country = p.BillToCountry,
                    zipCode = p.BillToPostalCode
                },
                shippingAddress = new
                {
                    contactName = p.CustomerName,
                    city = p.ShipToCity,
                    country = p.ShipToCountry,
                    address = p.ShipToAddressLine,
                    zipCode = p.ShipToPostalCode
                },
                billingAddress = new
                {
                    contactName = p.CustomerName,
                    city = p.BillToCity,
                    country = p.BillToCountry,
                    address = p.BillToAddressLine,
                    zipCode = p.BillToPostalCode
                },
                basketItems = new[]
                {
                    new
                    {
                        id = p.OrderNumber,
                        name = "Siparis",
                        category1 = "Genel",
                        itemType = "PHYSICAL",
                        price = p.Amount.ToString("F2", CultureInfo.InvariantCulture)
                    }
                },
                callbackUrl = p.ReturnUrl
            };

            var iyzicoResponse = await MakeRequestAsync<IyzicoInitResponse>("POST", GetUriPath("/payment/3dsecure/initialize", _isTestMode), requestBody);
            if (iyzicoResponse == null)
                return Result<PaymentGatewayInitializeResponseDto>.Failure("Iyzico yanıtı alınamadı.");

            var unified = new PaymentGatewayInitializeResponseDto
            {
                Success = iyzicoResponse.IsSuccess,
                ConversationId = iyzicoResponse.ConversationId,
                RedirectHtml = iyzicoResponse.HtmlContent,
                RedirectUrl = iyzicoResponse.PaymentPageUrl,
                ErrorCode = iyzicoResponse.ErrorCode,
                ErrorMessage = iyzicoResponse.ErrorMessage,
                LocalizedErrorMessage = iyzicoResponse.LocalizedErrorMessage,
                ErrorGroup = iyzicoResponse.ErrorGroup,
                Message = iyzicoResponse.LocalizedErrorMessage ?? iyzicoResponse.ErrorMessage ?? string.Empty
            };

            if (!unified.Success)
            {
                Logger.LogError("Iyzico InitializePayment failed: {ErrorCode}, {ErrorMessage}",
                    iyzicoResponse.ErrorCode, iyzicoResponse.ErrorMessage);
                return Result<PaymentGatewayInitializeResponseDto>.Failure(
                    iyzicoResponse.LocalizedErrorMessage ?? iyzicoResponse.ErrorMessage ?? "Ödeme başlatılamadı.",
                    unified);
            }

            return Result<PaymentGatewayInitializeResponseDto>.Success(unified);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Iyzico InitializePayment exception");
            return Result<PaymentGatewayInitializeResponseDto>.Failure($"Ödeme başlatılırken hata: {ex.Message}");
        }
    }

    /// <summary>Iyzico callback alanlarını standart callback DTO'suna map eder.</summary>
    public override Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request)
    {
        try
        {
            var status = request.IsSuccess
                ? "success"
                : (request.RawData.TryGetValue("status", out var st) ? st : "failure");

            var isSuccess = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);
            var response = new PaymentGatewayCallbackResponseDto
            {
                Success = isSuccess,
                PaymentId = request.PaymentId ?? request.RawData.GetValueOrDefault("paymentId") ?? string.Empty,
                OrderNumber = request.ConversationId ?? request.RawData.GetValueOrDefault("conversationId") ?? string.Empty,
                PaymentStatus = request.PaymentStatus ?? request.RawData.GetValueOrDefault("paymentStatus") ?? string.Empty,
                ErrorMessage = request.ErrorMessage ?? request.LocalizedErrorMessage ?? string.Empty,
                Message = isSuccess ? "Callback alındı" : "Callback başarısız"
            };

            if (isSuccess)
                return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Success(response));

            Logger.LogWarning("Iyzico callback failed: PaymentId={PaymentId}", response.PaymentId);
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure(
                request.LocalizedErrorMessage ?? request.ErrorMessage ?? "Ödeme callback başarısız."));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Iyzico ProcessCallback exception");
            return Task.FromResult(Result<PaymentGatewayCallbackResponseDto>.Failure($"Callback işlenirken hata: {ex.Message}"));
        }
    }

    /// <summary>Iyzico payment/detail API ile ödeme durumunu sorgular.</summary>
    public override async Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId)
    {
        try
        {
            if (!await InitializeIyzicoSettingsAsync())
                return Result<PaymentGatewayStatusResponseDto>.Failure("Iyzico ayarları yüklenemedi.");

            var requestBody = new { locale = "tr", paymentId };
            var responseContent = await MakeRequestAsyncRaw("POST", GetUriPath("/payment/detail", _isTestMode), requestBody);
            if (string.IsNullOrEmpty(responseContent))
                return Result<PaymentGatewayStatusResponseDto>.Failure("Iyzico yanıtı alınamadı.");

            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var response = new PaymentGatewayStatusResponseDto
            {
                PaymentId = root.TryGetProperty("paymentId", out var pid) ? pid.GetString() ?? paymentId : paymentId,
                Status = root.TryGetProperty("status", out var st) ? st.GetString() ?? string.Empty : string.Empty,
                PaymentStatus = root.TryGetProperty("paymentStatus", out var ps) ? ps.GetString() ?? string.Empty : string.Empty,
                ConversationId = root.TryGetProperty("conversationId", out var cid) ? cid.GetString() : null,
                PaidAmount = ParseJsonDecimal(root, "paidPrice"),
                Currency = root.TryGetProperty("currency", out var cur) ? cur.GetString() : null,
                ResponseCode = root.TryGetProperty("paymentStatus", out var rc) ? rc.GetString() ?? string.Empty : string.Empty,
                Raw = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)
            };
            response.Success = !string.Equals(response.Status, "failure", StringComparison.OrdinalIgnoreCase);

            if (!response.Success)
            {
                Logger.LogError("Iyzico GetPaymentStatus failed: {Status}, {PaymentId}", response.Status, paymentId);
                return Result<PaymentGatewayStatusResponseDto>.Failure("Ödeme durumu sorgulanamadı.");
            }

            return Result<PaymentGatewayStatusResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Iyzico GetPaymentStatus exception");
            return Result<PaymentGatewayStatusResponseDto>.Failure($"Durum sorgusu hatası: {ex.Message}");
        }
    }

    /// <summary>Iyzico installment API ile BIN/tutar bazlı taksit listesi döndürür.</summary>
    public override async Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request)
    {
        try
        {
            if (!await InitializeIyzicoSettingsAsync())
                return Result<PaymentGatewayInstallmentResponseDto>.Failure("Iyzico ayarları yüklenemedi.");

            var bin = (request.BinNumber ?? request.CardNumber ?? "").Trim().Replace(" ", "");
            if (bin.Length >= 6) bin = bin[..6];

            var requestBody = new
            {
                locale = request.Locale,
                price = request.Amount.ToString("F2", CultureInfo.InvariantCulture),
                binNumber = string.IsNullOrWhiteSpace(bin) ? null : bin,
                conversationId = string.IsNullOrWhiteSpace(request.ConversationId) ? null : request.ConversationId
            };

            var responseContent = await MakeRequestAsyncRaw("POST", GetUriPath("/payment/iyzipos/installment", _isTestMode), requestBody);
            if (string.IsNullOrEmpty(responseContent))
                return Result<PaymentGatewayInstallmentResponseDto>.Failure("Iyzico yanıtı alınamadı.");

            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var unified = new PaymentGatewayInstallmentResponseDto
            {
                Success = root.TryGetProperty("status", out var st) && st.GetString() == "success",
                ErrorCode = root.TryGetProperty("errorCode", out var ec) ? ec.GetString() : null,
                ErrorMessage = root.TryGetProperty("errorMessage", out var em) ? em.GetString() : null,
                LocalizedErrorMessage = root.TryGetProperty("localizedErrorMessage", out var lem) ? lem.GetString() : null
            };

            if (!unified.Success)
            {
                return Result<PaymentGatewayInstallmentResponseDto>.Failure(
                    unified.LocalizedErrorMessage ?? unified.ErrorMessage ?? "Taksit sorgusu başarısız.");
            }

            if (root.TryGetProperty("installmentDetails", out var details) && details.ValueKind == JsonValueKind.Array)
            {
                foreach (var detail in details.EnumerateArray())
                {
                    if (!detail.TryGetProperty("installmentPrices", out var prices) || prices.ValueKind != JsonValueKind.Array)
                        continue;

                    foreach (var price in prices.EnumerateArray())
                    {
                        if (!price.TryGetProperty("installmentNumber", out var numEl))
                            continue;

                        unified.Installments.Add(new InstallmentOptionDto
                        {
                            Count = numEl.GetInt32(),
                            Total = ParseJsonDecimal(price, "totalPrice") ?? 0,
                            Monthly = ParseJsonDecimal(price, "installmentPrice") ?? 0,
                            Label = $"{numEl.GetInt32()} Taksit"
                        });
                    }
                }
            }

            return Result<PaymentGatewayInstallmentResponseDto>.Success(unified);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Iyzico GetInstallmentInfo exception");
            return Result<PaymentGatewayInstallmentResponseDto>.Failure($"Taksit sorgusu hatası: {ex.Message}");
        }
    }

    /// <summary>3D doğrulama sonrası Iyzico auth API ile ödemeyi tamamlar; fraud kontrolü uygular.</summary>
    public override async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request)
    {
        try
        {
            if (!await InitializeIyzicoSettingsAsync())
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Iyzico ayarları yüklenemedi.");

            var requestBody = new
            {
                locale = request.Locale,
                paymentId = request.PaymentId,
                conversationId = request.ConversationId,
                conversationData = string.IsNullOrWhiteSpace(request.ConversationData) ? null : request.ConversationData
            };

            var responseContent = await MakeRequestAsyncRaw("POST", GetUriPath("/payment/3dsecure/auth", _isTestMode), requestBody);
            if (string.IsNullOrEmpty(responseContent))
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure("Iyzico yanıtı alınamadı.");

            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var unified = new PaymentGatewayAuth3DSResponseDto
            {
                Status = GetString(root, "status"),
                PaymentId = GetString(root, "paymentId", request.PaymentId),
                PaymentStatus = GetString(root, "paymentStatus"),
                ConversationId = root.TryGetProperty("conversationId", out var cid) ? cid.GetString() : request.ConversationId,
                ErrorCode = root.TryGetProperty("errorCode", out var ec) ? ec.GetString() : null,
                ErrorMessage = root.TryGetProperty("errorMessage", out var em) ? em.GetString() ?? string.Empty : string.Empty,
                LocalizedErrorMessage = root.TryGetProperty("localizedErrorMessage", out var lem) ? lem.GetString() : null,
                Price = ParseJsonDecimal(root, "price"),
                PaidAmount = ParseJsonDecimal(root, "paidPrice"),
                Installment = ParseJsonInt(root, "installment"),
                FraudStatus = ParseJsonInt(root, "fraudStatus"),
                CardType = root.TryGetProperty("cardType", out var ct) ? ct.GetString() : null,
                CardAssociation = root.TryGetProperty("cardAssociation", out var ca) ? ca.GetString() : null,
                CardFamily = root.TryGetProperty("cardFamily", out var cf) ? cf.GetString() : null,
                BinNumber = root.TryGetProperty("binNumber", out var bn) ? bn.GetString() : null,
                LastFourDigits = root.TryGetProperty("lastFourDigits", out var l4) ? l4.GetString() : null,
                Currency = root.TryGetProperty("currency", out var cur) ? cur.GetString() : null,
                OrderNumber = request.ConversationId ?? string.Empty
            };
            unified.Success = string.Equals(unified.Status, "success", StringComparison.OrdinalIgnoreCase);

            if (!unified.Success)
            {
                return Result<PaymentGatewayAuth3DSResponseDto>.Failure(
                    unified.LocalizedErrorMessage ?? unified.ErrorMessage ?? "Ödeme tamamlanamadı.");
            }

            if (!IyzicoPaymentHelper.IsMerchantApproved(unified.Status, unified.PaymentStatus, unified.FraudStatus))
            {
                var fraudMessage = IyzicoPaymentHelper.GetFraudUserMessage(unified.FraudStatus);
                unified.ErrorMessage = fraudMessage;
                unified.LocalizedErrorMessage = fraudMessage;
                Logger.LogWarning("Iyzico Auth3DS fraud bekleniyor: PaymentId={PaymentId}, FraudStatus={FraudStatus}",
                    unified.PaymentId, unified.FraudStatus);
            }

            unified.Message = "Ödeme tamamlandı";
            return Result<PaymentGatewayAuth3DSResponseDto>.Success(unified);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Iyzico Auth3DS exception");
            return Result<PaymentGatewayAuth3DSResponseDto>.Failure($"Auth3DS hatası: {ex.Message}");
        }
    }

    /// <summary>Iyzico iade API'si henüz bağlanmadı; başarısız sonuç döner.</summary>
    public override Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null)
        => Task.FromResult(Result<PaymentGatewayRefundResponseDto>.Failure("Iyzico iade henüz implemente edilmedi."));

    /// <summary>Iyzico form callback ham alanlarını status/paymentId/conversationId alanlarına çevirir.</summary>
    public override (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        if (rawData.Count == 0)
            return (null, null, null, null, null, null);

        var statusRaw = rawData.GetValueOrDefault("status");
        var status = string.IsNullOrEmpty(statusRaw)
            ? null
            : (statusRaw.Equals("success", StringComparison.OrdinalIgnoreCase) ? "success" : "failure");

        return (
            status,
            rawData.GetValueOrDefault("paymentId"),
            rawData.GetValueOrDefault("conversationId"),
            rawData.GetValueOrDefault("paymentStatus"),
            rawData.GetValueOrDefault("errorCode"),
            rawData.GetValueOrDefault("errorMessage") ?? rawData.GetValueOrDefault("localizedErrorMessage"));
    }

    private async Task<bool> InitializeIyzicoSettingsAsync()
    {
        try
        {
            var gatewayConfig = await GetGatewayConfigAsync();
            if (gatewayConfig == null)
            {
                Logger.LogError("Iyzico ayarları bulunamadı.");
                return false;
            }

            if (!gatewayConfig.Settings.TryGetValue("ApiKey", out _apiKey)
                || !gatewayConfig.Settings.TryGetValue("SecretKey", out _secretKey)
                || string.IsNullOrWhiteSpace(_apiKey)
                || string.IsNullOrWhiteSpace(_secretKey))
            {
                Logger.LogError("Iyzico API anahtarları eksik.");
                return false;
            }

            _isTestMode = gatewayConfig.IsTestMode;
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Iyzico ayarları yüklenirken hata");
            return false;
        }
    }

    private static string GetUriPath(string path, bool isTestMode)
    {
        var baseUrl = isTestMode ? SandboxBaseUrl : ProductionBaseUrl;
        return baseUrl + path;
    }

    private async Task<string> MakeRequestAsyncRaw(string method, string endpoint, object? requestBody = null)
    {
        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            throw new InvalidOperationException("Iyzico ayarları yüklenmemiş.");

        string? requestBodyJson = null;
        if (requestBody != null)
        {
            requestBodyJson = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }

        var uri = new Uri(endpoint);
        var uriPath = uri.AbsolutePath.TrimEnd('/');
        var authorizationHeader = IyzicoAuthorizationHelper.Generate(_apiKey, _secretKey, uriPath, requestBodyJson);

        return await MakeRequestAsyncRaw(
            endpoint,
            new HttpMethod(method),
            requestBodyJson,
            new Dictionary<string, string> { [AuthorizationHeader] = authorizationHeader });
    }

    private async Task<T?> MakeRequestAsync<T>(string method, string endpoint, object? requestBody = null) where T : class
    {
        var responseContent = await MakeRequestAsyncRaw(method, endpoint, requestBody);
        return string.IsNullOrWhiteSpace(responseContent)
            ? null
            : JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private static string NormalizeExpireYear(string year)
    {
        var digits = new string(year.Where(char.IsDigit).ToArray());
        return digits.Length >= 4 ? digits[^4..] : digits;
    }

    private static decimal? ParseJsonDecimal(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var el))
            return null;
        if (el.ValueKind == JsonValueKind.Number && el.TryGetDecimal(out var d))
            return d;
        if (el.ValueKind == JsonValueKind.String && decimal.TryParse(el.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            return parsed;
        return null;
    }

    private static int? ParseJsonInt(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var el))
            return null;
        if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var i))
            return i;
        if (el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out var parsed))
            return parsed;
        return null;
    }

    private static string GetString(JsonElement root, string property, string fallback = "")
        => root.TryGetProperty(property, out var el) ? el.GetString() ?? fallback : fallback;
}

using TriPay.Core.Common;
using TriPay.Core.Idempotency;
using TriPay.Services.Interfaces;
using TriPay.Services.Models;

namespace TriPay.Services;

/// <summary><see cref="IPaymentGatewayService"/> uygulaması; tüm ödeme çağrılarını <see cref="PaymentGatewayFactory"/> üzerinden yönlendirir.</summary>
public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly PaymentGatewayFactory _factory;
    private readonly IIdempotencyStore _idempotency;

    /// <summary>Factory ve idempotency deposu ile servis örneği oluşturur.</summary>
    public PaymentGatewayService(PaymentGatewayFactory factory, IIdempotencyStore idempotency)
    {
        _factory = factory;
        _idempotency = idempotency;
    }

    /// <summary>İstenen gateway için desteklenen provider'ı çözer; bulunamazsa istisna fırlatır.</summary>
    private async Task<IPaymentGatewayProvider> GetProviderAsync(string? gatewayName)
    {
        return await _factory.GetGatewayProviderAsync(gatewayName)
               ?? throw new InvalidOperationException("Payment gateway provider bulunamadı.");
    }

    /// <summary>İlgili gateway provider üzerinden ödeme başlatma isteğini yönlendirir.</summary>
    public async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName ?? request.GatewayName);
        return await provider.InitializePaymentAsync(request);
    }

    /// <summary>Callback işler; Redis idempotency ile tekrarlayan istekleri engeller.</summary>
    public async Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName ?? request.GatewayName);
        var normalized = provider.NormalizeCallbackFromRawData(request.RawData);
        var paymentId = request.PaymentId ?? normalized.PaymentId ?? string.Empty;
        var status = normalized.Status ?? (request.IsSuccess ? "success" : "failure");
        var idempotencyKey = IdempotencyKeyBuilder.ForCallback(provider.GatewayName, paymentId, status);

        var cached = await _idempotency.TryGetProcessedAsync<PaymentGatewayCallbackResponseDto>(idempotencyKey);
        if (cached != null)
            return cached;

        var result = await provider.ProcessCallbackAsync(request);
        await _idempotency.SaveProcessedAsync(idempotencyKey, result);
        return result;
    }

    /// <summary>Seçilen gateway ile ödeme durumu sorgusu yapar.</summary>
    public async Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName);
        return await provider.GetPaymentStatusAsync(paymentId);
    }

    /// <summary>Taksit seçenekleri sorgusunu ilgili provider'a iletir.</summary>
    public async Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName ?? request.GatewayName);
        return await provider.GetInstallmentInfoAsync(request);
    }

    /// <summary>3D sonrası Auth3DS tamamlama; idempotency ile tekrar çağrıları önler.</summary>
    public async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName);
        if (string.IsNullOrWhiteSpace(request.PaymentId))
        {
            var normalized = provider.NormalizeCallbackFromRawData(request.RawData);
            request.PaymentId = normalized.PaymentId ?? string.Empty;
        }

        var idempotencyKey = IdempotencyKeyBuilder.ForAuth3DS(provider.GatewayName, request.PaymentId);
        var cached = await _idempotency.TryGetProcessedAsync<PaymentGatewayAuth3DSResponseDto>(idempotencyKey);
        if (cached != null)
            return cached;

        var result = await provider.Auth3DSAsync(request);
        await _idempotency.SaveProcessedAsync(idempotencyKey, result);
        return result;
    }

    /// <summary>İade talebini seçilen gateway provider'a yönlendirir.</summary>
    public async Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName);
        return await provider.RefundPaymentAsync(paymentId, amount);
    }

    /// <summary>Ham callback sözlüğünü provider'a özgü alanlara normalize ettirir.</summary>
    public async Task<(string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)> NormalizeCallbackFromRawDataAsync(string? gatewayName, IReadOnlyDictionary<string, string> rawData)
    {
        var provider = await GetProviderAsync(gatewayName);
        return provider.NormalizeCallbackFromRawData(rawData);
    }

    /// <summary>Yapılandırmada etkin ve desteklenen gateway kodlarını listeler.</summary>
    public async Task<IReadOnlyList<string>> GetActiveGatewaysAsync()
    {
        var active = new List<string>();
        foreach (var gatewayName in _factory.GetAllAvailableGateways())
        {
            if (await _factory.GetGatewayProviderAsync(gatewayName) != null)
                active.Add(gatewayName);
        }

        return active;
    }

    /// <summary>Sistemde aktif işaretli provider gateway adlarını döndürür.</summary>
    public IReadOnlyList<string> GetSystemActiveGatewayNames()
    {
        return _factory.GetSystemActiveGatewayNames();
    }
}

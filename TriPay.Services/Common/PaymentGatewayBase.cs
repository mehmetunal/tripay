using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Interfaces;
using TriPay.Services.Models;

namespace TriPay.Core.Common;

/// <summary>Tüm sanal POS provider'larının ortak sözleşme ve log altyapısını sağlayan soyut taban sınıftır.</summary>
public abstract class PaymentGatewayBase : IPaymentGatewayProvider
{
    /// <summary>Provider içi loglama için kullanılan logger örneğidir.</summary>
    protected readonly ILogger Logger;

    /// <summary>Logger ile taban provider örneği oluşturur.</summary>
    /// <param name="logger">Kategori bazlı logger.</param>
    protected PaymentGatewayBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>Factory ve config eşlemesinde kullanılan kanal kodudur.</summary>
    public abstract string GatewayName { get; }

    /// <summary>Yönetim paneli ve loglarda görünen okunabilir kanal adıdır.</summary>
    public abstract string DisplayName { get; }

    /// <summary>Kanalın sistem genelinde etkin olup olmadığını belirtir.</summary>
    public virtual bool IsSystemActive => true;

    /// <summary>Kanalın yapılandırmaya göre kullanılabilir olup olmadığını kontrol eder.</summary>
    public virtual Task<bool> IsSupportedAsync() => Task.FromResult(IsSystemActive);

    /// <summary>Ödeme veya 3D Secure başlatma isteğini banka/kuruluşa iletir.</summary>
    public abstract Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request);

    /// <summary>Banka callback verisini standart callback DTO'suna dönüştürür.</summary>
    public abstract Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request);

    /// <summary>Ödeme kimliği veya sipariş numarası ile işlem durumunu sorgular.</summary>
    public abstract Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId);

    /// <summary>Kart BIN ve tutara göre taksit seçeneklerini listeler.</summary>
    public abstract Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request);

    /// <summary>3D Secure doğrulaması sonrası ödemeyi tamamlar (Auth3DS).</summary>
    public abstract Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request);

    /// <summary>Tam veya kısmi iade talebi gönderir.</summary>
    public abstract Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null);

    /// <summary>Ham callback alanlarını kanala özgü standart alanlara normalize eder; varsayılan boş döner.</summary>
    public virtual (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        return (null, null, null, null, null, null);
    }
}

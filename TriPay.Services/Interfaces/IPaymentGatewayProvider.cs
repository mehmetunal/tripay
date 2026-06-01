using TriPay.Core.Common;
using TriPay.Core.Gateways;
using TriPay.Services.Models;

namespace TriPay.Services.Interfaces;

/// <summary>Tek bir sanal POS kanalının (iyzico, Vakıfbank, VakıfPayS vb.) ödeme sözleşmesini tanımlar.</summary>
public interface IPaymentGatewayProvider
{
    /// <summary>Factory ve config eşlemesinde kullanılan kanal kodudur (<see cref="PaymentGatewayNames"/>).</summary>
    string GatewayName { get; }

    /// <summary>Yönetim paneli ve loglarda görünen okunabilir kanal adıdır.</summary>
    string DisplayName { get; }

    /// <summary>Kanalın sistem genelinde etkin olup olmadığını belirtir.</summary>
    bool IsSystemActive { get; }

    /// <summary>Kanalın yapılandırma ve ayarlara göre kullanılabilir olup olmadığını kontrol eder.</summary>
    Task<bool> IsSupportedAsync();

    /// <summary>Ödeme veya 3D Secure başlatma isteğini ilgili banka/kuruluşa iletir.</summary>
    Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request);

    /// <summary>Bankadan dönen callback verisini standart callback DTO'suna dönüştürür.</summary>
    Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request);

    /// <summary>Ödeme kimliği veya sipariş numarası ile işlem durumunu sorgular.</summary>
    Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId);

    /// <summary>Kart BIN/tutarına göre taksit seçeneklerini listeler.</summary>
    Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request);

    /// <summary>3D Secure doğrulaması sonrası ödemeyi tamamlar (Auth3DS).</summary>
    Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request);

    /// <summary>Tam veya kısmi iade talebi gönderir.</summary>
    Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null);

    /// <summary>Ham callback form alanlarını kanala özgü standart alanlara normalize eder.</summary>
    (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData);
}

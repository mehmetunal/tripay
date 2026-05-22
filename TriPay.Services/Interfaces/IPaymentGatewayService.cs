using TriPay.Core.Common;
using TriPay.Services.Models;

namespace TriPay.Services.Interfaces;

/// <summary>Uygulama katmanının tek giriş noktasından tüm ödeme kanallarına erişmesini sağlayan facade servisidir.</summary>
public interface IPaymentGatewayService
{
    /// <summary>Seçilen gateway üzerinden ödeme veya 3D başlatır.</summary>
    Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request, string? gatewayName = null);

    /// <summary>Callback isteğini ilgili provider'a yönlendirir.</summary>
    Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request, string? gatewayName = null);

    /// <summary>Ödeme durumunu sorgular.</summary>
    Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId, string? gatewayName = null);

    /// <summary>Taksit bilgisini sorgular.</summary>
    Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request, string? gatewayName = null);

    /// <summary>3D Secure sonrası ödeme tamamlama (Auth3DS) çağrısı yapar.</summary>
    Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request, string? gatewayName = null);

    /// <summary>İade işlemi başlatır.</summary>
    Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null, string? gatewayName = null);

    /// <summary>Ham callback verisini normalize eder.</summary>
    Task<(string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)> NormalizeCallbackFromRawDataAsync(string? gatewayName, IReadOnlyDictionary<string, string> rawData);

    /// <summary>Yapılandırmada etkin ve desteklenen gateway kodlarını listeler.</summary>
    Task<IReadOnlyList<string>> GetActiveGatewaysAsync();

    /// <summary>Sistemde kayıtlı ve aktif işaretli gateway adlarını döndürür.</summary>
    IReadOnlyList<string> GetSystemActiveGatewayNames();
}

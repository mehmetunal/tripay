using TriPay.Services.Common;
using TriPay.Services.Models;

namespace TriPay.Services.Interfaces;

public interface IPaymentGatewayService
{
    Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request, string? gatewayName = null);
    Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request, string? gatewayName = null);
    Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId, string? gatewayName = null);
    Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request, string? gatewayName = null);
    Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request, string? gatewayName = null);
    Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null, string? gatewayName = null);
    Task<(string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)> NormalizeCallbackFromRawDataAsync(string? gatewayName, IReadOnlyDictionary<string, string> rawData);
    Task<IReadOnlyList<string>> GetActiveGatewaysAsync();
    IReadOnlyList<string> GetSystemActiveGatewayNames();
}

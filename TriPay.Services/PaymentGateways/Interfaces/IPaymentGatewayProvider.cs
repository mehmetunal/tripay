using TriPay.Services.Common;
using TriPay.Services.PaymentGateways.Models;

namespace TriPay.Services.PaymentGateways.Interfaces;

public interface IPaymentGatewayProvider
{
    string GatewayName { get; }
    string DisplayName { get; }
    bool IsSystemActive { get; }

    Task<bool> IsSupportedAsync();
    Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request);
    Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request);
    Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId);
    Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request);
    Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request);
    Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null);
    (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData);
}

using Microsoft.Extensions.Logging;
using TriPay.Services.Common;
using TriPay.Services.Interfaces;
using TriPay.Services.Models;

namespace TriPay.Services.Common;

public abstract class PaymentGatewayBase : IPaymentGatewayProvider
{
    protected readonly ILogger Logger;

    protected PaymentGatewayBase(ILogger logger)
    {
        Logger = logger;
    }

    public abstract string GatewayName { get; }
    public abstract string DisplayName { get; }
    public virtual bool IsSystemActive => true;

    public virtual Task<bool> IsSupportedAsync() => Task.FromResult(IsSystemActive);

    public abstract Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request);
    public abstract Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request);
    public abstract Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId);
    public abstract Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request);
    public abstract Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request);
    public abstract Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null);

    public virtual (string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage) NormalizeCallbackFromRawData(IReadOnlyDictionary<string, string> rawData)
    {
        return (null, null, null, null, null, null);
    }
}

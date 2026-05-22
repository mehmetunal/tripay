using TriPay.Services.Common;
using TriPay.Services.Interfaces;
using TriPay.Services.Models;

namespace TriPay.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly PaymentGatewayFactory _factory;

    public PaymentGatewayService(PaymentGatewayFactory factory)
    {
        _factory = factory;
    }

    private async Task<IPaymentGatewayProvider> GetProviderAsync(string? gatewayName)
    {
        return await _factory.GetGatewayProviderAsync(gatewayName)
               ?? throw new InvalidOperationException("Payment gateway provider bulunamadı.");
    }

    public async Task<Result<PaymentGatewayInitializeResponseDto>> InitializePaymentAsync(PaymentGatewayInitializeRequestDto request, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName ?? request.GatewayName);
        return await provider.InitializePaymentAsync(request);
    }

    public async Task<Result<PaymentGatewayCallbackResponseDto>> ProcessCallbackAsync(PaymentGatewayCallbackRequestDto request, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName ?? request.GatewayName);
        return await provider.ProcessCallbackAsync(request);
    }

    public async Task<Result<PaymentGatewayStatusResponseDto>> GetPaymentStatusAsync(string paymentId, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName);
        return await provider.GetPaymentStatusAsync(paymentId);
    }

    public async Task<Result<PaymentGatewayInstallmentResponseDto>> GetInstallmentInfoAsync(PaymentGatewayInstallmentRequestDto request, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName ?? request.GatewayName);
        return await provider.GetInstallmentInfoAsync(request);
    }

    public async Task<Result<PaymentGatewayAuth3DSResponseDto>> Auth3DSAsync(PaymentGatewayAuth3DSRequestDto request, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName);
        return await provider.Auth3DSAsync(request);
    }

    public async Task<Result<PaymentGatewayRefundResponseDto>> RefundPaymentAsync(string paymentId, decimal? amount = null, string? gatewayName = null)
    {
        var provider = await GetProviderAsync(gatewayName);
        return await provider.RefundPaymentAsync(paymentId, amount);
    }

    public async Task<(string? Status, string? PaymentId, string? ConversationId, string? PaymentStatus, string? ErrorCode, string? ErrorMessage)> NormalizeCallbackFromRawDataAsync(string? gatewayName, IReadOnlyDictionary<string, string> rawData)
    {
        var provider = await GetProviderAsync(gatewayName);
        return provider.NormalizeCallbackFromRawData(rawData);
    }

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

    public IReadOnlyList<string> GetSystemActiveGatewayNames()
    {
        return _factory.GetSystemActiveGatewayNames();
    }
}

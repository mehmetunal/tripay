using TriPay.Core.Common;
using TriPay.Services.Models;
using TriPay.Services.Providers.VakifPays.Models;

namespace TriPay.Services.Checkout;

/// <summary>Ödeme akışı: DB işlem kaydı, loglama ve gateway delegasyonu.</summary>
public interface IPaymentCheckoutService
{
    /// <summary>Ödemeyi başlatır; işlem ve log kayıtlarını oluşturur.</summary>
    Task<Result<PaymentGatewayInitializeResponseDto>> PayAsync(PaymentRequest model, string gatewayName, CancellationToken cancellationToken = default);

    /// <summary>3D callback işler; tutar doğrulaması veritabanından yapılır.</summary>
    Task<CheckoutCallbackResult> ProcessCallbackAsync(Dictionary<string, string> rawData, string gatewayName, CancellationToken cancellationToken = default);

    /// <summary>Taksit bilgisini sorgular ve loglar.</summary>
    Task<Result<InstallmentInfoResponse>> GetInstallmentsAsync(string cardNumber, decimal amount, string gatewayName, CancellationToken cancellationToken = default);
}

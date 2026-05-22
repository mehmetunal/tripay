using TriPay.Services.Providers.VakifPays.Models;

namespace TriPay.Services.Models;

/// <summary>Ödeme başlatma isteğinin ortak gövdesidir; kart ve müşteri bilgisi <see cref="Payment"/> içindedir.</summary>
public class PaymentInitializeRequest
{
    /// <summary>Kart, tutar ve müşteri alanlarını içeren ödeme detayıdır.</summary>
    public PaymentRequest Payment { get; set; } = new();

    /// <summary>Kullanılacak gateway kodu; boşsa varsayılan kanal seçilir.</summary>
    public string? GatewayName { get; set; }
}

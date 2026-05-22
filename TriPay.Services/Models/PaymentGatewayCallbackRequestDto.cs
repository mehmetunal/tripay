namespace TriPay.Services.Models;

/// <summary>Gateway callback işleme isteği; ham veriye ek olarak Iyzico vb. için ayrıştırılmış alanlar içerir.</summary>
public class PaymentGatewayCallbackRequestDto : PaymentCallbackRequest
{
    /// <summary>İşlenecek gateway kodu.</summary>
    public string? GatewayName { get; set; }

    /// <summary>Önceden bilinen başarı bayrağı (formdan veya middleware'den).</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Banka ödeme kimliği.</summary>
    public string? PaymentId { get; set; }

    /// <summary>Sipariş veya conversation kimliği.</summary>
    public string? ConversationId { get; set; }

    /// <summary>Banka ödeme durum kodu veya metni.</summary>
    public string? PaymentStatus { get; set; }

    /// <summary>Banka hata kodu.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Banka hata mesajı.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Yerelleştirilmiş hata mesajı (Iyzico).</summary>
    public string? LocalizedErrorMessage { get; set; }
}

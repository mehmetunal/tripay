namespace TriPay.Services.Models;

/// <summary>3D Secure doğrulaması sonrası ödeme tamamlama (Auth3DS) isteğidir.</summary>
public class PaymentGatewayAuth3DSRequestDto : PaymentCallbackRequest
{
    /// <summary>API locale değeri (ör. tr).</summary>
    public string Locale { get; set; } = "tr";

    /// <summary>Banka ödeme kimliği.</summary>
    public string PaymentId { get; set; } = string.Empty;

    /// <summary>Sipariş veya conversation kimliği.</summary>
    public string? ConversationId { get; set; }

    /// <summary>Iyzico conversationData alanı.</summary>
    public string? ConversationData { get; set; }
}

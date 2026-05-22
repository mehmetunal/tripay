namespace TriPay.Services.Models;

/// <summary>Standartlaştırılmış callback yanıt modelidir; tüm kanallar bu yapıya map edilir.</summary>
public class PaymentGatewayCallbackResponseDto
{
    /// <summary>Callback'in başarılı kabul edilip edilmediği.</summary>
    public bool Success { get; set; }

    /// <summary><see cref="Success"/> ile aynı; eski API uyumluluğu.</summary>
    public bool IsSuccess { get => Success; set => Success = value; }

    /// <summary>Özet mesaj.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Üye işyeri sipariş numarası.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary><see cref="OrderNumber"/> ile aynı; Iyzico conversationId uyumu.</summary>
    public string? ConversationId { get => OrderNumber; set => OrderNumber = value ?? string.Empty; }

    /// <summary>Banka işlem veya pgTranId değeri.</summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary><see cref="TransactionId"/> ile aynı; Iyzico paymentId uyumu.</summary>
    public string? PaymentId { get => TransactionId; set => TransactionId = value ?? string.Empty; }

    /// <summary>Banka ödeme durumu (SUCCESS, PENDING vb.).</summary>
    public string? PaymentStatus { get; set; }

    /// <summary>Tahsil edilen tutar (varsa).</summary>
    public decimal? PaidAmount { get; set; }

    /// <summary>Para birimi.</summary>
    public string? Currency { get; set; }

    /// <summary>Banka yanıt kodu (ör. 00).</summary>
    public string ResponseCode { get; set; } = string.Empty;

    /// <summary>Hata veya red açıklaması.</summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

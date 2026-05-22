namespace TriPay.Services.Models;

/// <summary>Ödeme durum sorgusu yanıt modelidir.</summary>
public class PaymentGatewayStatusResponseDto
{
    /// <summary>Sorgunun başarılı olup olmadığı.</summary>
    public bool Success { get; set; }

    /// <summary><see cref="Success"/> ile aynı; eski API uyumluluğu.</summary>
    public bool IsSuccess { get => Success; set => Success = value; }

    /// <summary>API status alanı (success/failure).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Özet mesaj veya banka açıklaması.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Banka ödeme kimliği.</summary>
    public string PaymentId { get; set; } = string.Empty;

    /// <summary>Banka ödeme durumu (SUCCESS vb.).</summary>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>Sipariş conversation kimliği.</summary>
    public string? ConversationId { get; set; }

    /// <summary>Tahsil edilen tutar.</summary>
    public decimal? PaidAmount { get; set; }

    /// <summary>Para birimi.</summary>
    public string? Currency { get; set; }

    /// <summary>Banka yanıt veya durum kodu (geriye uyumluluk).</summary>
    public string ResponseCode { get; set; } = string.Empty;

    /// <summary>Ham banka yanıtı (debug ve genişletme için).</summary>
    public Dictionary<string, object>? Raw { get; set; }
}

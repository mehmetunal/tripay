namespace TriPay.Services.Models;

/// <summary>Ödeme başlatma sonrası yönlendirme HTML/URL ve durum bilgisini döndürür.</summary>
public class PaymentGatewayInitializeResponseDto
{
    /// <summary>Banka tarafında işlemin kabul edilip edilmediğini belirtir.</summary>
    public bool Success { get; set; }

    /// <summary><see cref="Success"/> ile aynı; Trimango uyumluluğu.</summary>
    public bool IsSuccess { get => Success; set => Success = value; }

    /// <summary>Kullanıcıya gösterilecek veya loglanacak mesajdır.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Banka ödeme kimliği (Iyzico initialize sonrası genelde boş).</summary>
    public string? PaymentId { get; set; }

    /// <summary>Sipariş / conversation kimliği.</summary>
    public string? ConversationId { get; set; }

    /// <summary>3D veya otomatik post için tarayıcıda render edilecek HTML gövdesidir.</summary>
    public string? RedirectHtml { get; set; }

    /// <summary>Trimango HtmlContent ile aynı.</summary>
    public string? HtmlContent { get => RedirectHtml; set => RedirectHtml = value; }

    /// <summary>Doğrudan yönlendirme URL'si (varsa).</summary>
    public string? RedirectUrl { get; set; }

    /// <summary>Trimango FormUrl ile aynı.</summary>
    public string? FormUrl { get => RedirectUrl; set => RedirectUrl = value; }

    /// <summary>Banka hata kodu.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Banka hata mesajı.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Yerelleştirilmiş hata mesajı.</summary>
    public string? LocalizedErrorMessage { get; set; }

    /// <summary>Hata grubu.</summary>
    public string? ErrorGroup { get; set; }
}

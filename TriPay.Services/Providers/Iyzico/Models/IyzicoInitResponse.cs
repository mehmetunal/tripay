namespace TriPay.Services.Providers.Iyzico.Models;

/// <summary>Iyzico /payment/3dsecure/initialize API JSON cevabı.</summary>
public sealed class IyzicoInitResponse
{
    /// <summary>İşlem durumu (success/failure).</summary>
    public string? Status { get; set; }

    /// <summary>Sipariş conversation kimliği.</summary>
    public string? ConversationId { get; set; }

    /// <summary>Tarayıcıda gösterilecek 3D HTML.</summary>
    public string? HtmlContent { get; set; }

    /// <summary>Alternatif ödeme sayfası URL'si.</summary>
    public string? PaymentPageUrl { get; set; }

    /// <summary>Banka hata kodu.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>İngilizce hata mesajı.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Yerelleştirilmiş hata mesajı.</summary>
    public string? LocalizedErrorMessage { get; set; }

    /// <summary>Hata grubu.</summary>
    public string? ErrorGroup { get; set; }

    /// <summary>Trimango IsSuccess ile uyumlu.</summary>
    public bool IsSuccess => string.Equals(Status, "success", StringComparison.OrdinalIgnoreCase);
}

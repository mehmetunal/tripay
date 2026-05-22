namespace TriPay.Services.Providers.VakifPays.Models;

/// <summary>
/// VakıfPayS iptal veya iade cevabını standartlaştırır.
/// </summary>
public class CancelRefundResponse
{
    /// <summary>İptal veya iade işleminin başarılı olup olmadığını belirtir.</summary>
    public bool Success { get; set; }

    /// <summary>Bankadan dönen açıklama mesajıdır.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Bankadan dönen ham cevabı taşır.</summary>
    public Dictionary<string, object>? Raw { get; set; }
}

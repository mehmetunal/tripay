namespace TriPay.Services.Models;

/// <summary>Ödeme başlatma yanıtı için temel alanları tanımlar (kanal DTO'ları bunu genişletir).</summary>
public class PaymentInitializeResponse
{
    /// <summary>İşlemin başlatılıp başlatılamadığını belirtir.</summary>
    public bool Success { get; set; }

    /// <summary>Kullanıcıya veya loga yazılacak kısa mesajdır.</summary>
    public string Message { get; set; } = string.Empty;
}

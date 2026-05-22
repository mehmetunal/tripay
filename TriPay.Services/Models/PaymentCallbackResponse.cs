namespace TriPay.Services.Models;

/// <summary>İşlenmiş callback yanıtı için temel alanları tanımlar.</summary>
public class PaymentCallbackResponse
{
    /// <summary>Callback'in başarılı sayılıp sayılmadığını belirtir.</summary>
    public bool Success { get; set; }

    /// <summary>Özet durum veya banka mesajıdır.</summary>
    public string Message { get; set; } = string.Empty;
}

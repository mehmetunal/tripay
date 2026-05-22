namespace TriPay.Services.Models;

/// <summary>Banka veya ödeme kuruluşundan gelen callback form/query alanlarını sözlük olarak taşır.</summary>
public class PaymentCallbackRequest
{
    /// <summary>Ham callback anahtar-değer çiftleridir (form alan adları bankaya göre değişir).</summary>
    public Dictionary<string, string> RawData { get; set; } = new();
}

namespace TriPay.Services.Providers.VakifPays.Models;

/// <summary>
/// VakıfPayS iptal veya iade işlemi için gönderilecek ortak istektir.
/// </summary>
public sealed class CancelRefundRequest
{
    /// <summary>İptal veya iade edilecek VakıfPayS işlem numarasıdır.</summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>İade edilecek tutardır; iptal işleminde banka tarafında zorunlu olmayabilir.</summary>
    public decimal Amount { get; set; }

    /// <summary>İşlem para birimidir.</summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>İşlemin test ortamında çalışıp çalışmayacağını belirler.</summary>
    public bool TestPlatform { get; set; } = true;
}

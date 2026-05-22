namespace TriPay.Data.Constants;

/// <summary>Transactions.Status değerleri (proje dokümanı §9.3).</summary>
public static class TransactionStatuses
{
    /// <summary>Ödeme başlatıldı, sonuç bekleniyor.</summary>
    public const string Pending = "Pending";

    /// <summary>Ödeme başarılı.</summary>
    public const string Success = "Success";

    /// <summary>Ödeme başarısız.</summary>
    public const string Failed = "Failed";

    /// <summary>İptal edildi.</summary>
    public const string Cancelled = "Cancelled";
}

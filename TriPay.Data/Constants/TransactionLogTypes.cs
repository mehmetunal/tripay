namespace TriPay.Data.Constants;

/// <summary>TransactionLogs.LogType değerleri (proje dokümanı §9.3).</summary>
public static class TransactionLogTypes
{
    /// <summary>Checkout Pay — gelen ödeme formu.</summary>
    public const string PayRequest = "PayRequest";

    /// <summary>Provider → banka ödeme başlatma isteği.</summary>
    public const string InitializeRequest = "InitializeRequest";

    /// <summary>Banka ödeme başlatma cevabı.</summary>
    public const string InitializeResponse = "InitializeResponse";

    /// <summary>Banka callback POST formu.</summary>
    public const string CallbackRequest = "CallbackRequest";

    /// <summary>Callback işleme sonucu.</summary>
    public const string CallbackResponse = "CallbackResponse";

    /// <summary>Ödeme durum sorgusu isteği.</summary>
    public const string QueryRequest = "QueryRequest";

    /// <summary>Ödeme durum sorgusu cevabı.</summary>
    public const string QueryResponse = "QueryResponse";

    /// <summary>İade isteği.</summary>
    public const string RefundRequest = "RefundRequest";

    /// <summary>İade cevabı.</summary>
    public const string RefundResponse = "RefundResponse";

    /// <summary>Taksit sorgu isteği.</summary>
    public const string InstallmentRequest = "InstallmentRequest";

    /// <summary>Taksit sorgu cevabı.</summary>
    public const string InstallmentResponse = "InstallmentResponse";
}

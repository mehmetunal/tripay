namespace TriPay.Data.Entities;

/// <summary>Ödeme API adımı istek/cevap logu.</summary>
public class PaymentTransactionLog
{
    /// <summary>Birincil anahtar.</summary>
    public long Id { get; set; }

    /// <summary>İşlem FK.</summary>
    public int TransactionId { get; set; }

    /// <summary><see cref="Constants.TransactionLogTypes"/> değeri.</summary>
    public string LogType { get; set; } = string.Empty;

    /// <summary><see cref="Constants.LogDirections"/> değeri.</summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>Giden istek (maskeli).</summary>
    public string? RequestPayload { get; set; }

    /// <summary>Gelen cevap.</summary>
    public string? ResponsePayload { get; set; }

    /// <summary>HTTP durum kodu.</summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>Gateway kodu.</summary>
    public string? GatewayCode { get; set; }

    /// <summary>Hata kodu.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Hata mesajı.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>İstek süresi (ms).</summary>
    public int? DurationMs { get; set; }

    /// <summary>Oluşturulma (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>İlişkili işlem.</summary>
    public PaymentTransaction? Transaction { get; set; }
}

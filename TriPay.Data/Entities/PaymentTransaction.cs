namespace TriPay.Data.Entities;

/// <summary>Ödeme özet kaydı (ham request/response burada tutulmaz).</summary>
public class PaymentTransaction
{
    /// <summary>Birincil anahtar.</summary>
    public int Id { get; set; }

    /// <summary>Üye işyeri FK.</summary>
    public int MerchantId { get; set; }

    /// <summary>Ödeme kanalı FK.</summary>
    public int PaymentGatewayId { get; set; }

    /// <summary>Üye işyeri sipariş numarası.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Banka işlem kimliği.</summary>
    public string? ExternalTransactionId { get; set; }

    /// <summary>İşlem tutarı.</summary>
    public decimal Amount { get; set; }

    /// <summary>Para birimi (ör. TRY).</summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>Taksit sayısı.</summary>
    public int? InstallmentCount { get; set; }

    /// <summary><see cref="Constants.TransactionStatuses"/> değeri.</summary>
    public string Status { get; set; } = Constants.TransactionStatuses.Pending;

    /// <summary>Normalize yanıt kodu.</summary>
    public string? ResponseCode { get; set; }

    /// <summary>Normalize yanıt mesajı.</summary>
    public string? ResponseMessage { get; set; }

    /// <summary>Müşteri IP.</summary>
    public string? ClientIp { get; set; }

    /// <summary>Callback idempotency anahtarı.</summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>Oluşturulma (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Güncellenme (UTC).</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>İlişkili üye işyeri.</summary>
    public Merchant? Merchant { get; set; }

    /// <summary>İlişkili ödeme kanalı.</summary>
    public PaymentGatewayRecord? PaymentGateway { get; set; }
}

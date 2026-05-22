namespace TriPay.Services.Messaging;

/// <summary>RabbitMQ <c>tripay.webhook.dispatch</c> kuyruğu mesaj sözleşmesi.</summary>
public sealed class PaymentWebhookMessage
{
    /// <summary>Tekil mesaj kimliği.</summary>
    public Guid MessageId { get; set; } = Guid.NewGuid();

    /// <summary>MSSQL işlem kimliği (ileride FK).</summary>
    public int TransactionId { get; set; }

    /// <summary>Üye işyeri kimliği.</summary>
    public int MerchantId { get; set; }

    /// <summary>Üye işyeri sipariş numarası.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>İşlem durumu (Success, Failed).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Tutar.</summary>
    public decimal Amount { get; set; }

    /// <summary>Para birimi.</summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>Olay zamanı (UTC).</summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Webhook worker retry sayacı.</summary>
    public int Attempt { get; set; }
}

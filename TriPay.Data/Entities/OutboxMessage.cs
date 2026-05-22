namespace TriPay.Data.Entities;

/// <summary>Transactional outbox — RabbitMQ webhook kuyruğu.</summary>
public class OutboxMessage
{
    /// <summary>Birincil anahtar.</summary>
    public long Id { get; set; }

    /// <summary>İlgili işlem FK.</summary>
    public int TransactionId { get; set; }

    /// <summary>Kuyruk mesaj gövdesi (JSON).</summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>RabbitMQ routing key.</summary>
    public string RoutingKey { get; set; } = "payment.webhook";

    /// <summary>Yayınlandı mı.</summary>
    public bool IsPublished { get; set; }

    /// <summary>Yayınlanma zamanı (UTC).</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>Yeniden deneme sayısı.</summary>
    public int RetryCount { get; set; }

    /// <summary>Oluşturulma (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}

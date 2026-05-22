namespace TriPay.Admin.Models.Outbox;

public sealed class OutboxListItem
{
    public long Id { get; init; }
    public int TransactionId { get; init; }
    public string RoutingKey { get; init; } = string.Empty;
    public bool IsPublished { get; init; }
    public int RetryCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public string PayloadPreview { get; init; } = string.Empty;
}

namespace TriPay.Admin.Application.Dtos.Outbox;

public sealed class OutboxListQueryDto
{
    public bool? UnpublishedOnly { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class OutboxListDto
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

public sealed class OutboxDetailDto
{
    public long Id { get; init; }
    public int TransactionId { get; init; }
    public string Payload { get; init; } = string.Empty;
    public string RoutingKey { get; init; } = string.Empty;
    public bool IsPublished { get; init; }
    public DateTime? PublishedAt { get; init; }
    public int RetryCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

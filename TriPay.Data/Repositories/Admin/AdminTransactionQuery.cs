namespace TriPay.Data.Repositories.Admin;

/// <summary>İşlem listesi sorgu parametreleri (repository katmanı).</summary>
public sealed class AdminTransactionQuery
{
    public string? OrderNumber { get; init; }
    public string? Status { get; init; }
    public int? MerchantId { get; init; }
    public int? PaymentGatewayId { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>İşlem listesi satırı.</summary>
public sealed record AdminTransactionListRow(
    int Id,
    string OrderNumber,
    string MerchantName,
    string GatewayCode,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt);

/// <summary>İşlem log satırı.</summary>
public sealed record AdminTransactionLogRow(
    long Id,
    string LogType,
    string Direction,
    string? GatewayCode,
    int? HttpStatusCode,
    string? ErrorCode,
    string? RequestPayload,
    string? ResponsePayload,
    DateTime CreatedAt);

/// <summary>Lookup (select list).</summary>
public sealed record AdminLookupRow(int Id, string Name);

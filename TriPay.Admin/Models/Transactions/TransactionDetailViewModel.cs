namespace TriPay.Admin.Models.Transactions;

/// <summary>İşlem detay ekranı.</summary>
public sealed class TransactionDetailViewModel
{
    public int Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string MerchantName { get; init; } = string.Empty;
    public string GatewayCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "TRY";
    public string Status { get; init; } = string.Empty;
    public string? ExternalTransactionId { get; init; }
    public string? ResponseCode { get; init; }
    public string? ResponseMessage { get; init; }
    public string? ClientIp { get; init; }
    public int? InstallmentCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<TransactionLogItem> Logs { get; init; } = [];
}

public sealed class TransactionLogItem
{
    public long Id { get; init; }
    public string LogType { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string? GatewayCode { get; init; }
    public int? HttpStatusCode { get; init; }
    public string? ErrorCode { get; init; }
    public string? RequestPayload { get; init; }
    public string? ResponsePayload { get; init; }
    public DateTime CreatedAt { get; init; }
}

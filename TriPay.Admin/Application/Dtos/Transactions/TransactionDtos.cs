namespace TriPay.Admin.Application.Dtos.Transactions;

public sealed class TransactionListQueryDto
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

public sealed class TransactionListDto
{
    public int Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string MerchantName { get; init; } = string.Empty;
    public string GatewayCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class TransactionLogDto
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

public sealed class TransactionDetailDto
{
    public int Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string MerchantName { get; init; } = string.Empty;
    public string GatewayCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ExternalTransactionId { get; init; }
    public string? ResponseCode { get; init; }
    public string? ResponseMessage { get; init; }
    public string? ClientIp { get; init; }
    public int? InstallmentCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<TransactionLogDto> Logs { get; init; } = [];
}

public sealed class TransactionIndexResultDto
{
    public required PagedResultDto<TransactionListDto> Page { get; init; }
    public required TransactionListQueryDto Query { get; init; }
    public IReadOnlyList<LookupDto> Merchants { get; init; } = [];
    public IReadOnlyList<LookupDto> Gateways { get; init; } = [];
}

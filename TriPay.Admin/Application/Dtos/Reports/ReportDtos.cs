namespace TriPay.Admin.Application.Dtos.Reports;

public sealed class ReportsFilterDto
{
    public DateTime FromUtc { get; init; }
    public DateTime ToUtc { get; init; }
    public int? MerchantId { get; init; }
    public int? PaymentGatewayId { get; init; }
}

public sealed class ReportSummaryDto
{
    public int TotalCount { get; init; }
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public int PendingCount { get; init; }
    public int CancelledCount { get; init; }
    public decimal SuccessAmount { get; init; }
    public string Currency { get; init; } = "TRY";
    public decimal SuccessRatePercent { get; init; }
}

public sealed class ReportMerchantRowDto
{
    public int MerchantId { get; init; }
    public string MerchantName { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public int SuccessCount { get; init; }
    public decimal SuccessAmount { get; init; }
    public string Currency { get; init; } = "TRY";
}

public sealed class ReportGatewayRowDto
{
    public int PaymentGatewayId { get; init; }
    public string GatewayCode { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public int SuccessCount { get; init; }
    public decimal SuccessAmount { get; init; }
    public string Currency { get; init; } = "TRY";
}

public sealed class ReportDailyRowDto
{
    public DateTime Date { get; init; }
    public int TotalCount { get; init; }
    public int SuccessCount { get; init; }
    public decimal SuccessAmount { get; init; }
}

public sealed class ReportsIndexDto
{
    public required ReportsFilterDto Filter { get; init; }
    public required ReportSummaryDto Summary { get; init; }
    public IReadOnlyList<ReportMerchantRowDto> ByMerchant { get; init; } = [];
    public IReadOnlyList<ReportGatewayRowDto> ByGateway { get; init; } = [];
    public IReadOnlyList<ReportDailyRowDto> DailyTrend { get; init; } = [];
    public IReadOnlyList<Dtos.LookupDto> Merchants { get; init; } = [];
    public IReadOnlyList<Dtos.LookupDto> Gateways { get; init; } = [];
}

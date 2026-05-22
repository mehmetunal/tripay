namespace TriPay.Data.Repositories.Admin;

/// <summary>Rapor sorgu parametreleri.</summary>
public sealed class AdminReportsQuery
{
    public DateTime FromUtc { get; init; }
    public DateTime ToUtc { get; init; }
    public int? MerchantId { get; init; }
    public int? PaymentGatewayId { get; init; }
}

public sealed record AdminReportSummaryRow(
    int TotalCount,
    int SuccessCount,
    int FailedCount,
    int PendingCount,
    int CancelledCount,
    decimal SuccessAmount,
    string Currency);

public sealed record AdminReportMerchantRow(
    int MerchantId,
    string MerchantName,
    int TotalCount,
    int SuccessCount,
    decimal SuccessAmount,
    string Currency);

public sealed record AdminReportGatewayRow(
    int PaymentGatewayId,
    string GatewayCode,
    int TotalCount,
    int SuccessCount,
    decimal SuccessAmount,
    string Currency);

public sealed record AdminReportDailyRow(
    DateTime Date,
    int TotalCount,
    int SuccessCount,
    decimal SuccessAmount);

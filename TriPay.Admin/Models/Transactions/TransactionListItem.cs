namespace TriPay.Admin.Models.Transactions;

/// <summary>İşlem liste satırı.</summary>
public sealed class TransactionListItem
{
    public int Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string MerchantName { get; init; } = string.Empty;
    public string GatewayCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "TRY";
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

using TriPay.Admin.Models.Shared;

namespace TriPay.Admin.Models.Transactions;

/// <summary>İşlem listesi filtreleri.</summary>
public sealed class TransactionListFilter : PaginationQuery
{
    public string? OrderNumber { get; set; }
    public string? Status { get; set; }
    public int? MerchantId { get; set; }
    public int? PaymentGatewayId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}

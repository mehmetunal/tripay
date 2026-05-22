namespace TriPay.Admin.Models.Reports;

/// <summary>Rapor filtre parametreleri (model binding).</summary>
public sealed class ReportsFilterModel
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int? MerchantId { get; set; }
    public int? PaymentGatewayId { get; set; }
}

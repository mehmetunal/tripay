namespace TriPay.Demo.Services;

/// <summary>Framework demosu: üye işyeri sipariş kaydı (gerçek projede kendi MSSQL'iniz).</summary>
public interface IDemoOrderStore
{
    void Save(DemoOrderRecord order);
    DemoOrderRecord? GetByOrderNumber(string orderNumber);
    void Update(DemoOrderRecord order);
}

/// <summary>Demo uygulamasındaki sipariş özeti.</summary>
public sealed class DemoOrderRecord
{
    public required string OrderNumber { get; init; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string Status { get; set; } = "Pending";
    public string? ExternalTransactionId { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

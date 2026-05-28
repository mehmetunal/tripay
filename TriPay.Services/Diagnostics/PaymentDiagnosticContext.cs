namespace TriPay.Services.Diagnostics;

/// <summary>Geçerli HTTP isteğindeki sipariş korelasyonu (AsyncLocal).</summary>
public static class PaymentDiagnosticContext
{
    private static readonly AsyncLocal<string?> OrderNumber = new();

    /// <summary>Logların bağlanacağı sipariş numarası.</summary>
    public static string? CurrentOrderNumber
    {
        get => OrderNumber.Value;
        set => OrderNumber.Value = value;
    }
}

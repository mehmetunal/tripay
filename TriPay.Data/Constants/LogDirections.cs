namespace TriPay.Data.Constants;

/// <summary>TransactionLogs.Direction değerleri.</summary>
public static class LogDirections
{
    /// <summary>TriPay → banka / dış sistem.</summary>
    public const string Outbound = "Outbound";

    /// <summary>Banka / dış sistem → TriPay.</summary>
    public const string Inbound = "Inbound";
}

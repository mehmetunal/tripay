namespace TriPay.Core.Idempotency;

/// <summary>Gateway callback ve Auth3DS için deterministik idempotency anahtarları üretir.</summary>
public static class IdempotencyKeyBuilder
{
    /// <summary>Banka callback tekrarı için anahtar.</summary>
    public static string ForCallback(string gatewayName, string paymentId, string? status)
        => $"callback:{gatewayName}:{paymentId}:{status ?? "unknown"}";

    /// <summary>3D Auth3DS tamamlama için anahtar.</summary>
    public static string ForAuth3DS(string gatewayName, string paymentId)
        => $"auth3ds:{gatewayName}:{paymentId}";

    /// <summary>Ödeme başlatma tekrarı için anahtar.</summary>
    public static string ForInitialize(string merchantKey, string orderNumber)
        => $"init:{merchantKey}:{orderNumber}";
}

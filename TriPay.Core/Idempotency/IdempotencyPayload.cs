namespace TriPay.Core.Idempotency;

/// <summary>Redis'e yazılan idempotency JSON gövdesi.</summary>
public sealed class IdempotencyPayload
{
    /// <summary>İşlem başarılı mı.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Hata mesajı.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Serileştirilmiş veri tipi.</summary>
    public string? DataType { get; set; }

    /// <summary>Serileştirilmiş veri JSON.</summary>
    public string? DataJson { get; set; }
}

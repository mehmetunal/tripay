namespace TriPay.Services.Diagnostics;

/// <summary>Tek bir ödeme tanılama kaydı (ekran / teknik destek).</summary>
public sealed class PaymentDiagnosticEntry
{
    /// <summary>Kayıt zamanı (UTC).</summary>
    public DateTime AtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Olay kategorisi (Checkout, 3D Giden, Callback, API vb.).</summary>
    public required string Category { get; init; }

    /// <summary>Gateway kodu.</summary>
    public string? Gateway { get; init; }

    /// <summary>Kısa başlık.</summary>
    public required string Title { get; init; }

    /// <summary>Çok satırlı detay gövdesi.</summary>
    public required string Detail { get; init; }

    /// <summary>Sipariş / oturum korelasyonu.</summary>
    public string? CorrelationId { get; init; }
}

using TriPay.Services.Diagnostics;

namespace TriPay.Demo.Models;

/// <summary>3D yönlendirme sayfası: olay günlüğü + banka HTML.</summary>
public sealed class ThreeDRedirectViewModel
{
    public required string OrderNumber { get; init; }
    public required string RedirectHtml { get; init; }
    public IReadOnlyList<PaymentDiagnosticEntry> Events { get; init; } = Array.Empty<PaymentDiagnosticEntry>();
}

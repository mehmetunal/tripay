using System.Collections.Concurrent;
using TriPay.Services.Diagnostics;

namespace TriPay.Demo.Services;

/// <summary>Demo: sipariş bazlı ödeme olay günlüğü (ekranda gösterim).</summary>
public sealed class DemoPaymentDiagnosticStore : IPaymentDiagnosticSink
{
    private const int MaxEntriesPerOrder = 120;
    private readonly ConcurrentDictionary<string, List<PaymentDiagnosticEntry>> _byOrder = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Write(PaymentDiagnosticEntry entry)
    {
        var key = entry.CorrelationId
            ?? PaymentDiagnosticContext.CurrentOrderNumber
            ?? "_global";

        var list = _byOrder.GetOrAdd(key, _ => new List<PaymentDiagnosticEntry>());
        lock (list)
        {
            list.Add(entry);
            if (list.Count > MaxEntriesPerOrder)
                list.RemoveRange(0, list.Count - MaxEntriesPerOrder);
        }
    }

    /// <summary>Siparişe ait kronolojik olay listesi.</summary>
    public IReadOnlyList<PaymentDiagnosticEntry> GetForOrder(string? orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return Array.Empty<PaymentDiagnosticEntry>();

        if (!_byOrder.TryGetValue(orderNumber.Trim(), out var list))
            return Array.Empty<PaymentDiagnosticEntry>();

        lock (list)
            return list.ToList();
    }

    /// <summary>
    /// Sipariş bulunamazsa global callback loglarını döner.
    /// </summary>
    public IReadOnlyList<PaymentDiagnosticEntry> GetForOrderWithGlobalFallback(string? orderNumber)
    {
        var byOrder = GetForOrder(orderNumber);
        if (byOrder.Count > 0)
            return byOrder;

        if (!_byOrder.TryGetValue("_global", out var global))
            return Array.Empty<PaymentDiagnosticEntry>();

        lock (global)
            return global.ToList();
    }

    /// <summary>Yeni ödeme başlamadan önce eski kayıtları temizler.</summary>
    public void ClearOrder(string orderNumber)
    {
        _byOrder.TryRemove(orderNumber.Trim(), out _);
    }
}

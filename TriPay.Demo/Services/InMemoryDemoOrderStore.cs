using System.Collections.Concurrent;

namespace TriPay.Demo.Services;

/// <summary>Framework demo için bellek içi sipariş deposu.</summary>
public sealed class InMemoryDemoOrderStore : IDemoOrderStore
{
    private readonly ConcurrentDictionary<string, DemoOrderRecord> _orders = new(StringComparer.OrdinalIgnoreCase);

    public void Save(DemoOrderRecord order) => _orders[order.OrderNumber] = order;

    public DemoOrderRecord? GetByOrderNumber(string orderNumber) =>
        _orders.TryGetValue(orderNumber, out var order) ? order : null;

    public void Update(DemoOrderRecord order) => _orders[order.OrderNumber] = order;
}

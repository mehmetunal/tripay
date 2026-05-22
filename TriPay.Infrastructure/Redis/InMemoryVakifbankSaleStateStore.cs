using System.Collections.Concurrent;
using TriPay.Core.Vakifbank;

namespace TriPay.Infrastructure.Redis;

/// <summary>Redis kapalıyken veya testlerde kullanılan bellek içi Vakıfbank satış deposu.</summary>
public sealed class InMemoryVakifbankSaleStateStore : IVakifbankSaleStateStore
{
    private readonly ConcurrentDictionary<string, VakifbankSaleState> _store = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Satış durumunu belleğe yazar.</summary>
    public Task SetAsync(string orderCode, VakifbankSaleState state, CancellationToken cancellationToken = default)
    {
        _store[orderCode] = state;
        return Task.CompletedTask;
    }

    /// <summary>Satış durumunu bellekten okur.</summary>
    public Task<VakifbankSaleState?> GetAsync(string orderCode, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(orderCode, out var state);
        return Task.FromResult(state);
    }

    /// <summary>Kaydı siler.</summary>
    public Task RemoveAsync(string orderCode, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(orderCode, out _);
        return Task.CompletedTask;
    }
}

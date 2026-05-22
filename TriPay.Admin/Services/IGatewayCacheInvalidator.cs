namespace TriPay.Admin.Services;

/// <summary>Gateway metadata Redis önbelleğini temizler.</summary>
public interface IGatewayCacheInvalidator
{
    /// <summary>Belirtilen kanal için settings + errors önbelleğini siler.</summary>
    Task InvalidateAsync(string gatewayCode, CancellationToken cancellationToken = default);

    /// <summary>Tüm kanallar için önbelleği temizler.</summary>
    Task InvalidateAllAsync(IEnumerable<string> gatewayCodes, CancellationToken cancellationToken = default);
}

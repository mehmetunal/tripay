using TriPay.Core.Redis;

namespace TriPay.Admin.Services;

/// <inheritdoc />
public sealed class GatewayCacheInvalidator : IGatewayCacheInvalidator
{
    private static readonly string[] Environments = ["Test", "Production"];
    private static readonly string[] Locales = ["tr", "en"];

    private readonly ITriPayRedisCache _redis;

    public GatewayCacheInvalidator(ITriPayRedisCache redis) => _redis = redis;

    /// <inheritdoc />
    public async Task InvalidateAsync(string gatewayCode, CancellationToken cancellationToken = default)
    {
        foreach (var env in Environments)
            await _redis.RemoveAsync(RedisKeyNames.GatewaySettings(gatewayCode, env), cancellationToken);

        foreach (var locale in Locales)
            await _redis.RemoveAsync(RedisKeyNames.GatewayErrors(gatewayCode, locale), cancellationToken);
    }

    /// <inheritdoc />
    public async Task InvalidateAllAsync(IEnumerable<string> gatewayCodes, CancellationToken cancellationToken = default)
    {
        foreach (var code in gatewayCodes.Distinct(StringComparer.OrdinalIgnoreCase))
            await InvalidateAsync(code, cancellationToken);
    }
}

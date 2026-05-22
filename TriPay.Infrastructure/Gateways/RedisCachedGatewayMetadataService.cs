using System.Text.Json;
using Microsoft.Extensions.Options;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Core.Redis;
using TriPay.Data.Repositories;

namespace TriPay.Infrastructure.Gateways;

/// <summary><see cref="IGatewayMetadataService"/> — MSSQL + Redis önbellek.</summary>
public sealed class RedisCachedGatewayMetadataService : IGatewayMetadataService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IGatewayMetadataRepository _repository;
    private readonly ITriPayRedisCache _redis;
    private readonly TriPayRedisOptions _options;

    /// <summary>Metadata servisi oluşturur.</summary>
    public RedisCachedGatewayMetadataService(
        IGatewayMetadataRepository repository,
        ITriPayRedisCache redis,
        IOptions<TriPayRedisOptions> options)
    {
        _repository = repository;
        _redis = redis;
        _options = options.Value;
    }

    /// <summary>Tek ayar değeri döner.</summary>
    public async Task<string?> GetSettingAsync(string gatewayCode, string settingKey, bool isTestMode, CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(gatewayCode, isTestMode, cancellationToken);
        return settings.TryGetValue(settingKey, out var value) ? value : null;
    }

    /// <summary>Provider hata kodunu kullanıcı mesajına çevirir.</summary>
    public async Task<string?> GetErrorMessageAsync(string gatewayCode, string? providerErrorCode, string locale = "tr", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(providerErrorCode))
            return null;

        var errors = await GetErrorMapAsync(gatewayCode, locale, cancellationToken);
        return errors.TryGetValue(providerErrorCode.Trim(), out var msg) ? msg : null;
    }

    /// <summary>Tüm hata eşlemelerini sözlük olarak döner.</summary>
    public Task<IReadOnlyDictionary<string, string>> GetErrorMapAsync(string gatewayCode, string locale = "tr", CancellationToken cancellationToken = default)
        => GetErrorMapInternalAsync(gatewayCode, locale, cancellationToken);

    /// <summary>Tüm ayarları sözlük olarak döner.</summary>
    public async Task<IReadOnlyDictionary<string, string>> GetSettingsAsync(string gatewayCode, bool isTestMode, CancellationToken cancellationToken = default)
    {
        var env = isTestMode ? "Test" : "Production";
        var cacheKey = RedisKeyNames.GatewaySettings(gatewayCode, env);
        var cached = await _redis.GetAsync<Dictionary<string, string>>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var gatewayId = await _repository.GetPaymentGatewayIdByCodeAsync(gatewayCode, cancellationToken);
        if (gatewayId == null)
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var rows = await _repository.GetSettingsAsync(gatewayId.Value, cancellationToken);
        var dict = BuildSettingsDictionary(rows, isTestMode);
        var ttl = TimeSpan.FromMinutes(Math.Max(1, _options.GatewayMetadataCacheMinutes));
        await _redis.SetAsync(cacheKey, dict, ttl, cancellationToken);
        return dict;
    }

    private async Task<IReadOnlyDictionary<string, string>> GetErrorMapInternalAsync(string gatewayCode, string locale, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeyNames.GatewayErrors(gatewayCode, locale);
        var cached = await _redis.GetAsync<Dictionary<string, string>>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var gatewayId = await _repository.GetPaymentGatewayIdByCodeAsync(gatewayCode, cancellationToken);
        if (gatewayId == null)
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var rows = await _repository.GetErrorMappingsAsync(gatewayId.Value, locale, cancellationToken);
        var dict = rows
            .GroupBy(e => e.ProviderErrorCode.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().UserMessage, StringComparer.OrdinalIgnoreCase);

        var ttl = TimeSpan.FromMinutes(Math.Max(1, _options.GatewayMetadataCacheMinutes));
        await _redis.SetAsync(cacheKey, dict, ttl, cancellationToken);
        return dict;
    }

    private static Dictionary<string, string> BuildSettingsDictionary(
        IReadOnlyList<TriPay.Data.Entities.GatewaySetting> rows,
        bool isTestMode)
    {
        var env = isTestMode ? "Test" : "Production";
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows.Where(r => r.Environment == "All"))
            dict[row.SettingKey] = row.SettingValue;

        foreach (var row in rows.Where(r => string.Equals(r.Environment, env, StringComparison.OrdinalIgnoreCase)))
            dict[row.SettingKey] = row.SettingValue;

        return dict;
    }
}

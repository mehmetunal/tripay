using TriPay.Admin.Application.Dtos.System;
using TriPay.Core.Redis;
using TriPay.Data.Repositories.Admin;

namespace TriPay.Admin.Application.Services;

/// <summary>Sistem durumu ve bakım işlemleri.</summary>
public sealed class AdminSystemService : IAdminSystemService
{
    private readonly IAdminDatabaseRepository _database;
    private readonly ITriPayRedisCache _redis;
    private readonly IAdminGatewayService _gateways;
    private readonly IConfiguration _configuration;

    public AdminSystemService(
        IAdminDatabaseRepository database,
        ITriPayRedisCache redis,
        IAdminGatewayService gateways,
        IConfiguration configuration)
    {
        _database = database;
        _redis = redis;
        _gateways = gateways;
        _configuration = configuration;
    }

    public async Task<SystemStatusDto> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var useInMemory = _configuration.GetValue<bool>("TriPay:Database:UseInMemory");
        long? version = null;

        if (!useInMemory)
            version = await _database.GetLatestMigrationVersionAsync(cancellationToken);

        return new SystemStatusDto
        {
            DatabaseOk = await _database.CanConnectAsync(cancellationToken),
            RedisOk = await _redis.PingAsync(cancellationToken),
            LatestMigrationVersion = version,
            LatestMigrationDescription = DescribeMigration(version),
            UseInMemoryDatabase = useInMemory,
            RabbitMqEnabled = _configuration.GetValue<bool>("TriPay:RabbitMq:Enabled"),
            AllowedIpRanges = _configuration.GetSection("TriPay:Admin:AllowedIpRanges").Get<string[]>() ?? []
        };
    }

    public Task ClearGatewayCachesAsync(CancellationToken cancellationToken = default) =>
        _gateways.InvalidateAllCachesAsync(cancellationToken);

    private static string? DescribeMigration(long? version) => version switch
    {
        null => null,
        202605220001 => "InitialSchema",
        202605220002 => "SeedData",
        202605220003 => "GatewayMetadataSchema",
        202605220004 => "GatewayMetadataSeed",
        202605220010 => "IdentitySchema",
        _ => $"Migration {version}"
    };
}

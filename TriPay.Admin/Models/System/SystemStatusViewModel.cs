namespace TriPay.Admin.Models.System;

/// <summary>Sistem durumu ekranı (salt okunur).</summary>
public sealed class SystemStatusViewModel
{
    public bool DatabaseOk { get; init; }
    public bool RedisOk { get; init; }
    public long? LatestMigrationVersion { get; init; }
    public string? LatestMigrationDescription { get; init; }
    public bool UseInMemoryDatabase { get; init; }
    public bool RabbitMqEnabled { get; init; }
    public IReadOnlyList<string> AllowedIpRanges { get; init; } = [];
}

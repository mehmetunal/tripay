namespace TriPay.Admin.Application.Dtos.System;

public sealed class SystemStatusDto
{
    public bool DatabaseOk { get; init; }
    public bool RedisOk { get; init; }
    public long? LatestMigrationVersion { get; init; }
    public string? LatestMigrationDescription { get; init; }
    public bool UseInMemoryDatabase { get; init; }
    public bool RabbitMqEnabled { get; init; }
    public IReadOnlyList<string> AllowedIpRanges { get; init; } = [];
}

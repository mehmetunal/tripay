using TriPay.Data.Entities;

namespace TriPay.Data.Repositories.Admin;

/// <summary>Gateway admin veri erişimi.</summary>
public interface IAdminGatewayRepository
{
    Task<IReadOnlyList<PaymentGatewayRecord>> ListGatewaysAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListGatewayCodesAsync(CancellationToken cancellationToken = default);
    Task<PaymentGatewayRecord?> GetGatewayAsync(int gatewayId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GatewaySetting>> ListSettingsAsync(int gatewayId, CancellationToken cancellationToken = default);
    Task<GatewaySettingEditRow?> GetSettingForEditAsync(int settingId, CancellationToken cancellationToken = default);
    Task<int> CreateSettingAsync(GatewaySettingUpsertData data, CancellationToken cancellationToken = default);
    Task<bool> UpdateSettingAsync(GatewaySettingUpsertData data, CancellationToken cancellationToken = default);
    Task<bool> DeleteSettingAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GatewayErrorMapping>> ListErrorsAsync(int gatewayId, CancellationToken cancellationToken = default);
    Task<GatewayErrorEditRow?> GetErrorForEditAsync(int errorId, CancellationToken cancellationToken = default);
    Task<int> CreateErrorAsync(GatewayErrorUpsertData data, CancellationToken cancellationToken = default);
    Task<bool> UpdateErrorAsync(GatewayErrorUpsertData data, CancellationToken cancellationToken = default);
    Task<bool> DeleteErrorAsync(int id, CancellationToken cancellationToken = default);
}

public sealed record GatewaySettingEditRow(
    int Id,
    int PaymentGatewayId,
    string GatewayCode,
    string SettingKey,
    string SettingValue,
    string Environment,
    bool IsActive);

public sealed record GatewayErrorEditRow(
    int Id,
    int PaymentGatewayId,
    string GatewayCode,
    string ProviderErrorCode,
    string? NormalizedCode,
    string UserMessage,
    string Locale,
    bool IsActive);

public sealed class GatewaySettingUpsertData
{
    public int Id { get; init; }
    public int PaymentGatewayId { get; init; }
    public required string SettingKey { get; init; }
    public required string SettingValue { get; init; }
    public required string Environment { get; init; }
    public bool IsActive { get; init; }
}

public sealed class GatewayErrorUpsertData
{
    public int Id { get; init; }
    public int PaymentGatewayId { get; init; }
    public required string ProviderErrorCode { get; init; }
    public string? NormalizedCode { get; init; }
    public required string UserMessage { get; init; }
    public required string Locale { get; init; }
    public bool IsActive { get; init; }
}

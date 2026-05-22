using TriPay.Admin.Application.Dtos.Gateways;

namespace TriPay.Admin.Application.Services;

public interface IAdminGatewayService
{
    Task<IReadOnlyList<GatewayListDto>> ListGatewaysAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListGatewayCodesAsync(CancellationToken cancellationToken = default);
    Task InvalidateAllCachesAsync(CancellationToken cancellationToken = default);

    Task<GatewayContextDto?> GetGatewayContextAsync(int gatewayId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GatewaySettingListDto>> ListSettingsAsync(int gatewayId, CancellationToken cancellationToken = default);
    Task<GatewaySettingEditDto?> GetSettingForEditAsync(int settingId, CancellationToken cancellationToken = default);
    Task<GatewaySettingEditDto> CreateSettingFormAsync(int gatewayId, CancellationToken cancellationToken = default);
    Task<int> CreateSettingAsync(UpsertGatewaySettingDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateSettingAsync(UpsertGatewaySettingDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSettingAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GatewayErrorListDto>> ListErrorsAsync(int gatewayId, CancellationToken cancellationToken = default);
    Task<GatewayErrorEditDto?> GetErrorForEditAsync(int errorId, CancellationToken cancellationToken = default);
    Task<GatewayErrorEditDto> CreateErrorFormAsync(int gatewayId, CancellationToken cancellationToken = default);
    Task<int> CreateErrorAsync(UpsertGatewayErrorDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateErrorAsync(UpsertGatewayErrorDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteErrorAsync(int id, CancellationToken cancellationToken = default);
}

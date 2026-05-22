using TriPay.Admin.Application.Dtos.Gateways;
using TriPay.Admin.Application.Mappings;
using TriPay.Admin.Services;
using TriPay.Data.Repositories.Admin;

namespace TriPay.Admin.Application.Services;

/// <summary>Gateway admin iş kuralları ve önbellek invalidasyonu.</summary>
public sealed class AdminGatewayService : IAdminGatewayService
{
    private readonly IAdminGatewayRepository _repository;
    private readonly IGatewayCacheInvalidator _cacheInvalidator;

    public AdminGatewayService(IAdminGatewayRepository repository, IGatewayCacheInvalidator cacheInvalidator)
    {
        _repository = repository;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<IReadOnlyList<GatewayListDto>> ListGatewaysAsync(CancellationToken cancellationToken = default)
    {
        var gateways = await _repository.ListGatewaysAsync(cancellationToken);
        return gateways.Select(AdminDtoMapper.ToGatewayListDto).ToList();
    }

    public Task<IReadOnlyList<string>> ListGatewayCodesAsync(CancellationToken cancellationToken = default) =>
        _repository.ListGatewayCodesAsync(cancellationToken);

    public async Task InvalidateAllCachesAsync(CancellationToken cancellationToken = default)
    {
        var codes = await _repository.ListGatewayCodesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateAllAsync(codes, cancellationToken);
    }

    public async Task<GatewayContextDto?> GetGatewayContextAsync(int gatewayId, CancellationToken cancellationToken = default)
    {
        var gateway = await _repository.GetGatewayAsync(gatewayId, cancellationToken);
        return gateway == null ? null : AdminDtoMapper.ToGatewayContext(gateway);
    }

    public async Task<IReadOnlyList<GatewaySettingListDto>> ListSettingsAsync(int gatewayId, CancellationToken cancellationToken = default)
    {
        var settings = await _repository.ListSettingsAsync(gatewayId, cancellationToken);
        return settings.Select(AdminDtoMapper.ToSettingListDto).ToList();
    }

    public async Task<GatewaySettingEditDto?> GetSettingForEditAsync(int settingId, CancellationToken cancellationToken = default)
    {
        var row = await _repository.GetSettingForEditAsync(settingId, cancellationToken);
        return row == null ? null : MapSettingEdit(row);
    }

    public async Task<GatewaySettingEditDto> CreateSettingFormAsync(int gatewayId, CancellationToken cancellationToken = default)
    {
        var gateway = await _repository.GetGatewayAsync(gatewayId, cancellationToken)
            ?? throw new InvalidOperationException("Gateway bulunamadı.");

        return new GatewaySettingEditDto
        {
            PaymentGatewayId = gateway.Id,
            GatewayCode = gateway.Code,
            Environment = "All",
            IsActive = true
        };
    }

    public async Task<int> CreateSettingAsync(UpsertGatewaySettingDto dto, CancellationToken cancellationToken = default)
    {
        var id = await _repository.CreateSettingAsync(new GatewaySettingUpsertData
        {
            PaymentGatewayId = dto.PaymentGatewayId,
            SettingKey = dto.SettingKey,
            SettingValue = dto.SettingValue,
            Environment = dto.Environment,
            IsActive = dto.IsActive
        }, cancellationToken);

        await _cacheInvalidator.InvalidateAsync(dto.GatewayCode, cancellationToken);
        return id;
    }

    public async Task<bool> UpdateSettingAsync(UpsertGatewaySettingDto dto, CancellationToken cancellationToken = default)
    {
        var ok = await _repository.UpdateSettingAsync(new GatewaySettingUpsertData
        {
            Id = dto.Id,
            PaymentGatewayId = dto.PaymentGatewayId,
            SettingKey = dto.SettingKey,
            SettingValue = dto.SettingValue,
            Environment = dto.Environment,
            IsActive = dto.IsActive
        }, cancellationToken);

        if (ok)
            await _cacheInvalidator.InvalidateAsync(dto.GatewayCode, cancellationToken);

        return ok;
    }

    public async Task<bool> DeleteSettingAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await _repository.GetSettingForEditAsync(id, cancellationToken);
        if (row == null)
            return false;

        var ok = await _repository.DeleteSettingAsync(id, cancellationToken);
        if (ok)
            await _cacheInvalidator.InvalidateAsync(row.GatewayCode, cancellationToken);

        return ok;
    }

    public async Task<IReadOnlyList<GatewayErrorListDto>> ListErrorsAsync(int gatewayId, CancellationToken cancellationToken = default)
    {
        var errors = await _repository.ListErrorsAsync(gatewayId, cancellationToken);
        return errors.Select(AdminDtoMapper.ToErrorListDto).ToList();
    }

    public async Task<GatewayErrorEditDto?> GetErrorForEditAsync(int errorId, CancellationToken cancellationToken = default)
    {
        var row = await _repository.GetErrorForEditAsync(errorId, cancellationToken);
        return row == null ? null : MapErrorEdit(row);
    }

    public async Task<GatewayErrorEditDto> CreateErrorFormAsync(int gatewayId, CancellationToken cancellationToken = default)
    {
        var gateway = await _repository.GetGatewayAsync(gatewayId, cancellationToken)
            ?? throw new InvalidOperationException("Gateway bulunamadı.");

        return new GatewayErrorEditDto
        {
            PaymentGatewayId = gateway.Id,
            GatewayCode = gateway.Code,
            Locale = "tr",
            IsActive = true
        };
    }

    public async Task<int> CreateErrorAsync(UpsertGatewayErrorDto dto, CancellationToken cancellationToken = default)
    {
        var id = await _repository.CreateErrorAsync(new GatewayErrorUpsertData
        {
            PaymentGatewayId = dto.PaymentGatewayId,
            ProviderErrorCode = dto.ProviderErrorCode,
            NormalizedCode = dto.NormalizedCode,
            UserMessage = dto.UserMessage,
            Locale = dto.Locale,
            IsActive = dto.IsActive
        }, cancellationToken);

        await _cacheInvalidator.InvalidateAsync(dto.GatewayCode, cancellationToken);
        return id;
    }

    public async Task<bool> UpdateErrorAsync(UpsertGatewayErrorDto dto, CancellationToken cancellationToken = default)
    {
        var ok = await _repository.UpdateErrorAsync(new GatewayErrorUpsertData
        {
            Id = dto.Id,
            PaymentGatewayId = dto.PaymentGatewayId,
            ProviderErrorCode = dto.ProviderErrorCode,
            NormalizedCode = dto.NormalizedCode,
            UserMessage = dto.UserMessage,
            Locale = dto.Locale,
            IsActive = dto.IsActive
        }, cancellationToken);

        if (ok)
            await _cacheInvalidator.InvalidateAsync(dto.GatewayCode, cancellationToken);

        return ok;
    }

    public async Task<bool> DeleteErrorAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await _repository.GetErrorForEditAsync(id, cancellationToken);
        if (row == null)
            return false;

        var ok = await _repository.DeleteErrorAsync(id, cancellationToken);
        if (ok)
            await _cacheInvalidator.InvalidateAsync(row.GatewayCode, cancellationToken);

        return ok;
    }

    private static GatewaySettingEditDto MapSettingEdit(GatewaySettingEditRow row) => new()
    {
        Id = row.Id,
        PaymentGatewayId = row.PaymentGatewayId,
        GatewayCode = row.GatewayCode,
        SettingKey = row.SettingKey,
        SettingValue = row.SettingValue,
        Environment = row.Environment,
        IsActive = row.IsActive
    };

    private static GatewayErrorEditDto MapErrorEdit(GatewayErrorEditRow row) => new()
    {
        Id = row.Id,
        PaymentGatewayId = row.PaymentGatewayId,
        GatewayCode = row.GatewayCode,
        ProviderErrorCode = row.ProviderErrorCode,
        NormalizedCode = row.NormalizedCode,
        UserMessage = row.UserMessage,
        Locale = row.Locale,
        IsActive = row.IsActive
    };
}

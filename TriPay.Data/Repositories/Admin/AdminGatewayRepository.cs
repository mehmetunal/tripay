using Microsoft.EntityFrameworkCore;
using TriPay.Data.Entities;
using TriPay.Data.Persistence;

namespace TriPay.Data.Repositories.Admin;

/// <summary><see cref="IAdminGatewayRepository"/> EF Core uygulaması.</summary>
public sealed class AdminGatewayRepository : IAdminGatewayRepository
{
    private readonly TriPayDbContext _db;

    public AdminGatewayRepository(TriPayDbContext db) => _db = db;

    public async Task<IReadOnlyList<PaymentGatewayRecord>> ListGatewaysAsync(CancellationToken cancellationToken = default) =>
        await _db.PaymentGateways.AsNoTracking().OrderBy(g => g.Code).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> ListGatewayCodesAsync(CancellationToken cancellationToken = default) =>
        await _db.PaymentGateways.AsNoTracking().Select(g => g.Code).ToListAsync(cancellationToken);

    public Task<PaymentGatewayRecord?> GetGatewayAsync(int gatewayId, CancellationToken cancellationToken = default) =>
        _db.PaymentGateways.AsNoTracking().FirstOrDefaultAsync(g => g.Id == gatewayId, cancellationToken);

    public async Task<IReadOnlyList<GatewaySetting>> ListSettingsAsync(int gatewayId, CancellationToken cancellationToken = default) =>
        await _db.GatewaySettings.AsNoTracking()
            .Where(s => s.PaymentGatewayId == gatewayId)
            .OrderBy(s => s.SettingKey)
            .ThenBy(s => s.Environment)
            .ToListAsync(cancellationToken);

    public Task<GatewaySettingEditRow?> GetSettingForEditAsync(int settingId, CancellationToken cancellationToken = default) =>
        _db.GatewaySettings.AsNoTracking()
            .Where(gs => gs.Id == settingId)
            .Join(_db.PaymentGateways, gs => gs.PaymentGatewayId, g => g.Id, (gs, g) => new GatewaySettingEditRow(
                gs.Id,
                gs.PaymentGatewayId,
                g.Code,
                gs.SettingKey,
                gs.SettingValue,
                gs.Environment,
                gs.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<int> CreateSettingAsync(GatewaySettingUpsertData data, CancellationToken cancellationToken = default)
    {
        var entity = new GatewaySetting
        {
            PaymentGatewayId = data.PaymentGatewayId,
            SettingKey = data.SettingKey,
            SettingValue = data.SettingValue,
            Environment = data.Environment,
            IsActive = data.IsActive
        };
        _db.GatewaySettings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateSettingAsync(GatewaySettingUpsertData data, CancellationToken cancellationToken = default)
    {
        var entity = await _db.GatewaySettings.FirstOrDefaultAsync(x => x.Id == data.Id, cancellationToken);
        if (entity == null)
            return false;

        entity.SettingKey = data.SettingKey;
        entity.SettingValue = data.SettingValue;
        entity.Environment = data.Environment;
        entity.IsActive = data.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteSettingAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.GatewaySettings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
            return false;

        _db.GatewaySettings.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<GatewayErrorMapping>> ListErrorsAsync(int gatewayId, CancellationToken cancellationToken = default) =>
        await _db.GatewayErrorMappings.AsNoTracking()
            .Where(e => e.PaymentGatewayId == gatewayId)
            .OrderBy(e => e.ProviderErrorCode)
            .ThenBy(e => e.Locale)
            .ToListAsync(cancellationToken);

    public Task<GatewayErrorEditRow?> GetErrorForEditAsync(int errorId, CancellationToken cancellationToken = default) =>
        _db.GatewayErrorMappings.AsNoTracking()
            .Where(e => e.Id == errorId)
            .Join(_db.PaymentGateways, e => e.PaymentGatewayId, g => g.Id, (e, g) => new GatewayErrorEditRow(
                e.Id,
                e.PaymentGatewayId,
                g.Code,
                e.ProviderErrorCode,
                e.NormalizedCode,
                e.UserMessage,
                e.Locale,
                e.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<int> CreateErrorAsync(GatewayErrorUpsertData data, CancellationToken cancellationToken = default)
    {
        var entity = new GatewayErrorMapping
        {
            PaymentGatewayId = data.PaymentGatewayId,
            ProviderErrorCode = data.ProviderErrorCode,
            NormalizedCode = data.NormalizedCode,
            UserMessage = data.UserMessage,
            Locale = data.Locale,
            IsActive = data.IsActive
        };
        _db.GatewayErrorMappings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateErrorAsync(GatewayErrorUpsertData data, CancellationToken cancellationToken = default)
    {
        var entity = await _db.GatewayErrorMappings.FirstOrDefaultAsync(x => x.Id == data.Id, cancellationToken);
        if (entity == null)
            return false;

        entity.ProviderErrorCode = data.ProviderErrorCode;
        entity.NormalizedCode = data.NormalizedCode;
        entity.UserMessage = data.UserMessage;
        entity.Locale = data.Locale;
        entity.IsActive = data.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteErrorAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.GatewayErrorMappings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
            return false;

        _db.GatewayErrorMappings.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

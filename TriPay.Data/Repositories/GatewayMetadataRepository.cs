using Microsoft.EntityFrameworkCore;
using TriPay.Data.Entities;
using TriPay.Data.Persistence;

namespace TriPay.Data.Repositories;

/// <summary><see cref="IGatewayMetadataRepository"/> EF Core uygulaması.</summary>
public sealed class GatewayMetadataRepository : IGatewayMetadataRepository
{
    private readonly TriPayDbContext _db;

    /// <summary>Repository oluşturur.</summary>
    public GatewayMetadataRepository(TriPayDbContext db) => _db = db;

    /// <summary>Kanal koduna göre gateway kimliğini döner.</summary>
    public Task<int?> GetPaymentGatewayIdByCodeAsync(string gatewayCode, CancellationToken cancellationToken = default)
        => _db.PaymentGateways
            .Where(g => g.Code == gatewayCode && g.IsActive)
            .Select(g => (int?)g.Id)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>Aktif gateway ayarlarını getirir.</summary>
    public async Task<IReadOnlyList<GatewaySetting>> GetSettingsAsync(int paymentGatewayId, CancellationToken cancellationToken = default)
        => await _db.GatewaySettings
            .Where(s => s.PaymentGatewayId == paymentGatewayId && s.IsActive)
            .ToListAsync(cancellationToken);

    /// <summary>Aktif hata eşlemelerini getirir.</summary>
    public async Task<IReadOnlyList<GatewayErrorMapping>> GetErrorMappingsAsync(int paymentGatewayId, string locale, CancellationToken cancellationToken = default)
        => await _db.GatewayErrorMappings
            .Where(e => e.PaymentGatewayId == paymentGatewayId && e.IsActive && e.Locale == locale)
            .ToListAsync(cancellationToken);
}

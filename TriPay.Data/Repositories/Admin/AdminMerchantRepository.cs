using Microsoft.EntityFrameworkCore;
using TriPay.Data.Entities;
using TriPay.Data.Persistence;

namespace TriPay.Data.Repositories.Admin;

/// <summary><see cref="IAdminMerchantRepository"/> EF Core uygulaması.</summary>
public sealed class AdminMerchantRepository : IAdminMerchantRepository
{
    private readonly TriPayDbContext _db;

    public AdminMerchantRepository(TriPayDbContext db) => _db = db;

    public async Task<IReadOnlyList<Merchant>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Merchants.AsNoTracking().OrderBy(m => m.Name).ToListAsync(cancellationToken);

    public Task<Merchant?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Merchants.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<bool> UpdateAsync(int id, string name, string? webhookUrl, bool isActive, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Merchants.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (entity == null)
            return false;

        entity.Name = name;
        entity.WebhookUrl = webhookUrl;
        entity.IsActive = isActive;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

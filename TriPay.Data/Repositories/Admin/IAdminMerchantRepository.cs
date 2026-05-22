using TriPay.Data.Entities;

namespace TriPay.Data.Repositories.Admin;

/// <summary>Üye işyeri admin veri erişimi.</summary>
public interface IAdminMerchantRepository
{
    Task<IReadOnlyList<Merchant>> ListAsync(CancellationToken cancellationToken = default);
    Task<Merchant?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, string name, string? webhookUrl, bool isActive, CancellationToken cancellationToken = default);
}

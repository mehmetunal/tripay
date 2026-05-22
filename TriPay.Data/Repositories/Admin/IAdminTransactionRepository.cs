using TriPay.Data.Entities;

namespace TriPay.Data.Repositories.Admin;

/// <summary>İşlem admin veri erişimi.</summary>
public interface IAdminTransactionRepository
{
    Task<AdminPagedResult<AdminTransactionListRow>> ListAsync(AdminTransactionQuery query, CancellationToken cancellationToken = default);
    Task<PaymentTransaction?> GetDetailAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminTransactionLogRow>> GetLogsAsync(int transactionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminLookupRow>> ListMerchantLookupsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminLookupRow>> ListGatewayLookupsAsync(CancellationToken cancellationToken = default);
}

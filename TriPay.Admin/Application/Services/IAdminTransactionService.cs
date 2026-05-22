using TriPay.Admin.Application.Dtos.Transactions;

namespace TriPay.Admin.Application.Services;

public interface IAdminTransactionService
{
    Task<TransactionIndexResultDto> GetIndexAsync(TransactionListQueryDto query, CancellationToken cancellationToken = default);
    Task<TransactionDetailDto?> GetDetailAsync(int id, CancellationToken cancellationToken = default);
}

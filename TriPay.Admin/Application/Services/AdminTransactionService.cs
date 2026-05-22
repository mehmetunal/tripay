using TriPay.Admin.Application.Dtos;
using TriPay.Admin.Application.Dtos.Transactions;
using TriPay.Admin.Application.Mappings;
using TriPay.Data.Repositories.Admin;

namespace TriPay.Admin.Application.Services;

/// <summary>İşlem admin iş kuralları.</summary>
public sealed class AdminTransactionService : IAdminTransactionService
{
    private readonly IAdminTransactionRepository _repository;

    public AdminTransactionService(IAdminTransactionRepository repository) => _repository = repository;

    public async Task<TransactionIndexResultDto> GetIndexAsync(
        TransactionListQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var page = await _repository.ListAsync(AdminDtoMapper.ToRepositoryQuery(query), cancellationToken);
        var merchants = await _repository.ListMerchantLookupsAsync(cancellationToken);
        var gateways = await _repository.ListGatewayLookupsAsync(cancellationToken);

        return new TransactionIndexResultDto
        {
            Query = query,
            Page = AdminDtoMapper.ToPagedDto(page, row => new TransactionListDto
            {
                Id = row.Id,
                OrderNumber = row.OrderNumber,
                MerchantName = row.MerchantName,
                GatewayCode = row.GatewayCode,
                Amount = row.Amount,
                Currency = row.Currency,
                Status = row.Status,
                CreatedAt = row.CreatedAt
            }),
            Merchants = merchants.Select(m => new LookupDto { Id = m.Id, Name = m.Name }).ToList(),
            Gateways = gateways.Select(g => new LookupDto { Id = g.Id, Name = g.Name }).ToList()
        };
    }

    public async Task<TransactionDetailDto?> GetDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var txn = await _repository.GetDetailAsync(id, cancellationToken);
        if (txn == null)
            return null;

        var logs = await _repository.GetLogsAsync(id, cancellationToken);

        return new TransactionDetailDto
        {
            Id = txn.Id,
            OrderNumber = txn.OrderNumber,
            MerchantName = txn.Merchant?.Name ?? "-",
            GatewayCode = txn.PaymentGateway?.Code ?? "-",
            Amount = txn.Amount,
            Currency = txn.Currency,
            Status = txn.Status,
            ExternalTransactionId = txn.ExternalTransactionId,
            ResponseCode = txn.ResponseCode,
            ResponseMessage = txn.ResponseMessage,
            ClientIp = txn.ClientIp,
            InstallmentCount = txn.InstallmentCount,
            CreatedAt = txn.CreatedAt,
            UpdatedAt = txn.UpdatedAt,
            Logs = logs.Select(l => new TransactionLogDto
            {
                Id = l.Id,
                LogType = l.LogType,
                Direction = l.Direction,
                GatewayCode = l.GatewayCode,
                HttpStatusCode = l.HttpStatusCode,
                ErrorCode = l.ErrorCode,
                RequestPayload = l.RequestPayload,
                ResponsePayload = l.ResponsePayload,
                CreatedAt = l.CreatedAt
            }).ToList()
        };
    }
}

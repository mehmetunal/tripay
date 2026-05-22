using TriPay.Admin.Application.Dtos;
using TriPay.Admin.Application.Dtos.Outbox;

namespace TriPay.Admin.Application.Services;

public interface IAdminOutboxService
{
    Task<PagedResultDto<OutboxListDto>> ListAsync(OutboxListQueryDto query, CancellationToken cancellationToken = default);
    Task<OutboxDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> RequeueAsync(long id, CancellationToken cancellationToken = default);
}

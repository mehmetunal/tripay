using TriPay.Admin.Application.Dtos;
using TriPay.Admin.Application.Dtos.Outbox;
using TriPay.Admin.Application.Mappings;
using TriPay.Data.Repositories.Admin;

namespace TriPay.Admin.Application.Services;

/// <summary>Outbox admin iş kuralları.</summary>
public sealed class AdminOutboxService : IAdminOutboxService
{
    private readonly IAdminOutboxRepository _repository;

    public AdminOutboxService(IAdminOutboxRepository repository) => _repository = repository;

    public async Task<PagedResultDto<OutboxListDto>> ListAsync(
        OutboxListQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var page = await _repository.ListAsync(new AdminOutboxQuery
        {
            UnpublishedOnly = query.UnpublishedOnly,
            Page = query.Page,
            PageSize = query.PageSize
        }, cancellationToken);

        return AdminDtoMapper.ToPagedDto(page, row => new OutboxListDto
        {
            Id = row.Id,
            TransactionId = row.TransactionId,
            RoutingKey = row.RoutingKey,
            IsPublished = row.IsPublished,
            RetryCount = row.RetryCount,
            CreatedAt = row.CreatedAt,
            PublishedAt = row.PublishedAt,
            PayloadPreview = row.PayloadPreview
        });
    }

    public async Task<OutboxDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return null;

        return new OutboxDetailDto
        {
            Id = item.Id,
            TransactionId = item.TransactionId,
            Payload = item.Payload,
            RoutingKey = item.RoutingKey,
            IsPublished = item.IsPublished,
            PublishedAt = item.PublishedAt,
            RetryCount = item.RetryCount,
            CreatedAt = item.CreatedAt
        };
    }

    public Task<bool> RequeueAsync(long id, CancellationToken cancellationToken = default) =>
        _repository.RequeueAsync(id, cancellationToken);
}

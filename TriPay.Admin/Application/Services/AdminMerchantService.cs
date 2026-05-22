using TriPay.Admin.Application.Dtos.Merchants;
using TriPay.Data.Repositories.Admin;

namespace TriPay.Admin.Application.Services;

/// <summary>Üye işyeri admin iş kuralları.</summary>
public sealed class AdminMerchantService : IAdminMerchantService
{
    private readonly IAdminMerchantRepository _repository;

    public AdminMerchantService(IAdminMerchantRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<MerchantListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _repository.ListAsync(cancellationToken);
        return rows.Select(m => new MerchantListDto
        {
            Id = m.Id,
            Name = m.Name,
            IsActive = m.IsActive,
            WebhookUrl = m.WebhookUrl,
            ApiKeyMasked = AdminApiKeyMasker.Mask(m.ApiKey),
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<MerchantEditDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetByIdAsync(id, cancellationToken);
        if (m == null)
            return null;

        return new MerchantEditDto
        {
            Id = m.Id,
            Name = m.Name,
            WebhookUrl = m.WebhookUrl,
            IsActive = m.IsActive,
            ApiKeyMasked = AdminApiKeyMasker.Mask(m.ApiKey),
            CreatedAt = m.CreatedAt
        };
    }

    public Task<bool> UpdateAsync(UpdateMerchantDto dto, CancellationToken cancellationToken = default) =>
        _repository.UpdateAsync(dto.Id, dto.Name, dto.WebhookUrl, dto.IsActive, cancellationToken);
}

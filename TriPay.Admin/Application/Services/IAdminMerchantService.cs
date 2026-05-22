using TriPay.Admin.Application.Dtos.Merchants;

namespace TriPay.Admin.Application.Services;

public interface IAdminMerchantService
{
    Task<IReadOnlyList<MerchantListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<MerchantEditDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateMerchantDto dto, CancellationToken cancellationToken = default);
}

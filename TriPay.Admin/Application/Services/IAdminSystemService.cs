using TriPay.Admin.Application.Dtos.System;

namespace TriPay.Admin.Application.Services;

public interface IAdminSystemService
{
    Task<SystemStatusDto> GetStatusAsync(CancellationToken cancellationToken = default);
    Task ClearGatewayCachesAsync(CancellationToken cancellationToken = default);
}

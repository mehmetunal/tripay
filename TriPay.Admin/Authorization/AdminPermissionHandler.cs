using Microsoft.AspNetCore.Authorization;
using TriPay.Data.Identity;

namespace TriPay.Admin.Authorization;

/// <summary>Admin rolü tüm izinlere sahiptir; diğer roller claim ile kontrol edilir.</summary>
public sealed class AdminPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Task.CompletedTask;

        if (context.User.IsInRole(AdminRole.Admin.ToRoleName()))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.HasClaim(AdminPermissions.ClaimType, requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

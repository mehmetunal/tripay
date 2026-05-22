using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TriPay.Data.Entities;

namespace TriPay.Data.Identity;

/// <summary>Kullanıcı oturumuna rol permission claim'lerini ekler (AspNetRoleClaims).</summary>
public sealed class ApplicationClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<int>>
{
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public ApplicationClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
        _roleManager = roleManager;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var roles = await UserManager.GetRolesAsync(user);

        foreach (var roleName in roles)
        {
            if (AdminRoles.IsAdminRole(roleName))
                continue;

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                continue;

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims.Where(c => c.Type == AdminPermissions.ClaimType))
            {
                if (!identity.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
                    identity.AddClaim(claim);
            }
        }

        return identity;
    }
}

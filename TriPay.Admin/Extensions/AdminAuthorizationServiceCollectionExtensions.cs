using Microsoft.AspNetCore.Authorization;
using TriPay.Admin.Authorization;
using TriPay.Data.Identity;

namespace TriPay.Admin.Extensions;

/// <summary>Panel yetki policy kayıtları.</summary>
public static class AdminAuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddTriPayAdminAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, AdminPermissionHandler>();

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(AdminPermissions.PanelAccess))
                .Build();

            foreach (var code in AdminPermissions.AllCodes)
            {
                options.AddPolicy(code, policy =>
                    policy.Requirements.Add(new PermissionRequirement(code)));
            }
        });

        return services;
    }
}

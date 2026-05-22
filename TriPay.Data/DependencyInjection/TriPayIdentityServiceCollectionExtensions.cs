using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TriPay.Data.Entities;
using TriPay.Data.Identity;
using TriPay.Data.Persistence;

namespace TriPay.Data.DependencyInjection;

/// <summary>TriPay Identity DI kayıtları (yönetim paneli).</summary>
public static class TriPayIdentityServiceCollectionExtensions
{
    /// <summary>ASP.NET Core Identity + EF store (aynı <see cref="TriPayDbContext"/>).</summary>
    public static IServiceCollection AddTriPayIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<TriPayDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationClaimsPrincipalFactory>();

        return services;
    }
}

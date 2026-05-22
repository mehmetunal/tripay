using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TriPay.Data.Entities;

namespace TriPay.Data.Identity;

/// <summary>Development ortamında varsayılan admin kullanıcı seed'i.</summary>
public static class AdminIdentitySeeder
{
    /// <summary>Geriye dönük uyumluluk.</summary>
    public const string AdminRoleName = nameof(AdminRole.Admin);

    public const string DefaultAdminEmail = "admin@gmail.com";
    public const string DefaultAdminPassword = "Super123!";

    /// <summary>Roller/izinler + (Development) varsayılan admin kullanıcı.</summary>
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await AdminPermissionSeeder.SeedAsync(services, cancellationToken);

        using var scope = services.CreateScope();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (!env.IsDevelopment())
            return;

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(AdminIdentitySeeder));
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(DefaultAdminEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = DefaultAdminEmail,
                Email = DefaultAdminEmail,
                EmailConfirmed = true,
                DisplayName = "TriPay Admin"
            };

            var createResult = await userManager.CreateAsync(user, DefaultAdminPassword);
            if (!createResult.Succeeded)
            {
                logger.LogWarning("Admin kullanıcı oluşturulamadı: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(user, AdminRole.Admin.ToRoleName()))
            await userManager.AddToRoleAsync(user, AdminRole.Admin.ToRoleName());

        logger.LogInformation("Development admin kullanıcı seed tamamlandı ({Email}).", DefaultAdminEmail);
    }
}

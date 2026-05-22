using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TriPay.Data.Identity;

/// <summary>Roller ve Kullanıcı rolü claim seed (veritabanı).</summary>
public static class AdminPermissionSeeder
{
    /// <summary>Admin + Kullanıcı rollerini ve Kullanıcı varsayılan izinlerini oluşturur/günceller.</summary>
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(AdminPermissionSeeder));
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        foreach (var roleName in AdminRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            if (!result.Succeeded)
                logger.LogWarning("Rol oluşturulamadı {Role}: {Errors}", roleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await SyncRoleClaimsAsync(roleManager, AdminRole.User.ToRoleName(), AdminPermissions.DefaultUserRoleCodes, logger, cancellationToken);
        logger.LogInformation("Admin rol/izin seed tamamlandı.");
    }

    /// <summary>Belirtilen rolün permission claim'lerini veritabanıyla senkronlar.</summary>
    public static async Task SyncRoleClaimsAsync(
        RoleManager<IdentityRole<int>> roleManager,
        string roleName,
        IEnumerable<string> permissionCodes,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
            return;

        var desired = permissionCodes.Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existing = await roleManager.GetClaimsAsync(role);
        var existingPerms = existing
            .Where(c => c.Type == AdminPermissions.ClaimType)
            .ToList();

        foreach (var claim in existingPerms.Where(c => !desired.Contains(c.Value)))
        {
            var remove = await roleManager.RemoveClaimAsync(role, claim);
            if (!remove.Succeeded && logger != null)
                logger.LogWarning("Claim silinemedi {Role}/{Perm}", roleName, claim.Value);
        }

        var have = existingPerms.Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var code in desired.Where(c => !have.Contains(c)))
        {
            var add = await roleManager.AddClaimAsync(role, new Claim(AdminPermissions.ClaimType, code));
            if (!add.Succeeded && logger != null)
                logger.LogWarning("Claim eklenemedi {Role}/{Perm}", roleName, code);
        }
    }
}

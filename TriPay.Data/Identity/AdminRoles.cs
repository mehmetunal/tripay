namespace TriPay.Data.Identity;

/// <summary><see cref="AdminRole"/> için Identity uyumlu sabitler ve koleksiyonlar.</summary>
public static class AdminRoles
{
    public static string Admin => AdminRole.Admin.ToRoleName();

    public static string User => AdminRole.User.ToRoleName();

    public static IReadOnlyList<string> All => AdminRoleExtensions.GetAll().Select(r => r.ToRoleName()).ToList();

    public static IReadOnlyList<AdminRole> AllRoles => AdminRoleExtensions.GetAll();

    public static string GetDisplayName(string roleName) => AdminRoleExtensions.GetDisplayName(roleName);

    public static bool TryParse(string? roleName, out AdminRole role) =>
        AdminRoleExtensions.TryParseRoleName(roleName, out role);

    public static bool IsAdminRole(string? roleName) =>
        TryParse(roleName, out var role) && role == AdminRole.Admin;
}

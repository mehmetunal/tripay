using System.ComponentModel;
using System.Reflection;

namespace TriPay.Data.Identity;

/// <summary><see cref="AdminRole"/> yardımcıları ve Description okuma.</summary>
public static class AdminRoleExtensions
{
    /// <summary>Identity rol adı (AspNetRoles.Name).</summary>
    public static string ToRoleName(this AdminRole role) => role.ToString();

    /// <summary><see cref="DescriptionAttribute"/> metni; yoksa enum adı.</summary>
    public static string GetDescription(this AdminRole role) => GetEnumDescription(role);

    /// <summary>Rol adından enum çözümler.</summary>
    public static bool TryParseRoleName(string? roleName, out AdminRole role) =>
        Enum.TryParse(roleName, ignoreCase: true, out role);

    /// <summary>Rol adı geçerli bir <see cref="AdminRole"/> mü?</summary>
    public static bool IsDefinedRoleName(string? roleName) =>
        TryParseRoleName(roleName, out _);

    /// <summary>Rol adından Türkçe etiket.</summary>
    public static string GetDisplayName(string? roleName) =>
        TryParseRoleName(roleName, out var role) ? role.GetDescription() : roleName ?? string.Empty;

    /// <summary>Tanımlı tüm roller.</summary>
    public static IReadOnlyList<AdminRole> GetAll() => Enum.GetValues<AdminRole>();

    private static string GetEnumDescription(Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var description = member?.GetCustomAttribute<DescriptionAttribute>();
        return description?.Description ?? value.ToString();
    }
}

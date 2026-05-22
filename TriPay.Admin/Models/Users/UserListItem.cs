namespace TriPay.Admin.Models.Users;

/// <summary>Panel kullanıcı liste satırı.</summary>
public sealed class UserListItem
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public bool EmailConfirmed { get; init; }
    public bool LockoutEnabled { get; init; }
    public bool IsLockedOut { get; init; }
}

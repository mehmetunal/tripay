namespace TriPay.Admin.Models.Roles;

public sealed class RoleListItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool IsAdminRole { get; init; }
    public int PermissionCount { get; init; }
}

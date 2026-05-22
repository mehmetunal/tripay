namespace TriPay.Admin.Models.Roles;

public sealed class RoleEditViewModel
{
    public int Id { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public List<RolePermissionItem> Permissions { get; set; } = [];
}

public sealed class RolePermissionItem
{
    public string Code { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsSelected { get; set; }
}

using Microsoft.AspNetCore.Authorization;

namespace TriPay.Admin.Authorization;

/// <summary>Belirli bir panel izni gerektirir.</summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission) => Permission = permission;

    public string Permission { get; }
}

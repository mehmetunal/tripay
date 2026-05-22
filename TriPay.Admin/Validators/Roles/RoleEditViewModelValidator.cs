using FluentValidation;
using TriPay.Admin.Models.Roles;
using TriPay.Data.Identity;

namespace TriPay.Admin.Validators.Roles;

public sealed class RoleEditViewModelValidator : AbstractValidator<RoleEditViewModel>
{
    public RoleEditViewModelValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.RoleName).NotEmpty();
        RuleFor(x => x.Permissions)
            .Must(HasPanelAccess)
            .WithMessage("En az 'Panele giriş' izni seçilmelidir.");
    }

    private static bool HasPanelAccess(List<RolePermissionItem>? permissions) =>
        permissions?.Any(p => p.IsSelected &&
                              string.Equals(p.Code, AdminPermissions.PanelAccess, StringComparison.OrdinalIgnoreCase)) == true;
}

using FluentValidation;
using FluentValidation.AspNetCore;
using TriPay.Admin.Validators.Account;
using TriPay.Admin.Validators.Gateways;
using TriPay.Admin.Validators.Merchants;
using TriPay.Admin.Validators.Roles;
using TriPay.Admin.Validators.Users;

namespace TriPay.Admin.Extensions;

public static class AdminFluentValidationServiceCollectionExtensions
{
    /// <summary>Admin MVC formları için FluentValidation (DataAnnotations devre dışı).</summary>
    public static IServiceCollection AddTriPayAdminFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginViewModelValidator>();

        services.AddFluentValidationAutoValidation(options =>
        {
            options.DisableDataAnnotationsValidation = true;
        });

        return services;
    }
}

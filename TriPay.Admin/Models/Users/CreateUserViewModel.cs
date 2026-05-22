using System.ComponentModel.DataAnnotations;
using TriPay.Data.Identity;

namespace TriPay.Admin.Models.Users;

/// <summary>Yeni panel kullanıcısı.</summary>
public sealed class CreateUserViewModel
{
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Görünen ad")]
    public string? DisplayName { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Şifre tekrar")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Rol")]
    public string RoleName { get; set; } = AdminRole.User.ToRoleName();
}

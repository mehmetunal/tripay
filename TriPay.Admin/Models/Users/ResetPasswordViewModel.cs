using System.ComponentModel.DataAnnotations;

namespace TriPay.Admin.Models.Users;

/// <summary>Admin tarafından şifre sıfırlama formu.</summary>
public sealed class ResetPasswordViewModel
{
    public int UserId { get; set; }

    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Yeni şifre")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Şifre tekrar")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

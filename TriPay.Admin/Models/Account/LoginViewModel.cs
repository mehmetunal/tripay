using System.ComponentModel.DataAnnotations;

namespace TriPay.Admin.Models.Account;

/// <summary>Giriş formu modeli.</summary>
public sealed class LoginViewModel
{
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Beni hatırla")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

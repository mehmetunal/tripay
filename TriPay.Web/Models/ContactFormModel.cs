using System.ComponentModel.DataAnnotations;

namespace TriPay.Web.Models;

public sealed class ContactFormModel
{
    [Display(Name = "Ad Soyad")]
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(120)]
    public string AdSoyad { get; set; } = "";

    [Display(Name = "E-posta")]
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
    public string Eposta { get; set; } = "";

    [Display(Name = "Şirket")]
    [StringLength(160)]
    public string? Sirket { get; set; }

    [Display(Name = "Konu")]
    [Required(ErrorMessage = "Konu seçin.")]
    public string Konu { get; set; } = "entegrasyon";

    [Display(Name = "Mesaj")]
    [Required(ErrorMessage = "Mesaj zorunludur.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Mesaj en az 10 karakter olmalıdır.")]
    public string Mesaj { get; set; } = "";
}

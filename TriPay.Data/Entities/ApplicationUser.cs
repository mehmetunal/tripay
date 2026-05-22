using Microsoft.AspNetCore.Identity;

namespace TriPay.Data.Entities;

/// <summary>TriPay yönetim paneli kullanıcısı (Identity).</summary>
public sealed class ApplicationUser : IdentityUser<int>
{
    /// <summary>Görünen ad.</summary>
    public string? DisplayName { get; set; }
}

using System.ComponentModel;

namespace TriPay.Data.Identity;

/// <summary>Yönetim paneli Identity rolleri (AspNetRoles.Name ile eşleşir).</summary>
public enum AdminRole
{
    /// <summary>Tam yetki — tüm izinler (claim kontrolü bypass).</summary>
    [Description("Yönetici (Admin)")]
    Admin,

    /// <summary>Sınırlı operasyon — rol claim'leri ile yetkilendirilir.</summary>
    [Description("Kullanıcı")]
    User
}

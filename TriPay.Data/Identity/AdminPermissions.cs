namespace TriPay.Data.Identity;

/// <summary>Panel yetki kodları (AspNetRoleClaims — ClaimType = <see cref="ClaimType"/>).</summary>
public static class AdminPermissions
{
    public const string ClaimType = "permission";

    public const string PanelAccess = "panel.access";
    public const string DashboardView = "dashboard.view";
    public const string TransactionsView = "transactions.view";
    public const string ReportsView = "reports.view";
    public const string MerchantsView = "merchants.view";
    public const string MerchantsManage = "merchants.manage";
    public const string GatewaysView = "gateways.view";
    public const string GatewaysManage = "gateways.manage";
    public const string OutboxView = "outbox.view";
    public const string OutboxManage = "outbox.manage";
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";
    public const string SystemView = "system.view";
    public const string SystemManage = "system.manage";

    /// <summary>Tüm tanımlı izinler (rol düzenleme UI).</summary>
    public static readonly IReadOnlyList<AdminPermissionDefinition> Definitions =
    [
        new(PanelAccess, "Panele giriş", "Giriş yapabilir"),
        new(DashboardView, "Dashboard", "Özet ekranı"),
        new(TransactionsView, "İşlemler", "İşlem listesi ve detay"),
        new(ReportsView, "Raporlar", "Özet ve kırılım raporları"),
        new(MerchantsView, "Üye işyeri görüntüleme", "Liste"),
        new(MerchantsManage, "Üye işyeri düzenleme", "Kayıt güncelleme"),
        new(GatewaysView, "Kanal görüntüleme", "Gateway listesi ve ayar okuma"),
        new(GatewaysManage, "Kanal yönetimi", "Ayar/hata CRUD, önbellek"),
        new(OutboxView, "Outbox görüntüleme", "Kuyruk listesi"),
        new(OutboxManage, "Outbox işlem", "Yeniden kuyruk"),
        new(UsersManage, "Kullanıcı yönetimi", "Kullanıcı CRUD"),
        new(RolesManage, "Rol ve yetki", "Rol izinleri düzenleme"),
        new(SystemView, "Sistem görüntüleme", "Durum ekranı"),
        new(SystemManage, "Sistem işlem", "Önbellek temizleme vb.")
    ];

    public static readonly string[] AllCodes = Definitions.Select(d => d.Code).ToArray();

    /// <summary><see cref="AdminRole.User"/> rolüne varsayılan atanacak salt okunur izinler.</summary>
    public static readonly string[] DefaultUserRoleCodes =
    [
        PanelAccess,
        DashboardView,
        TransactionsView,
        ReportsView,
        MerchantsView,
        GatewaysView,
        OutboxView,
        SystemView
    ];

    public static string GetLabel(string code) =>
        Definitions.FirstOrDefault(d => d.Code == code)?.Label ?? code;
}

/// <summary>Yetki meta verisi.</summary>
public sealed record AdminPermissionDefinition(string Code, string Label, string Description);

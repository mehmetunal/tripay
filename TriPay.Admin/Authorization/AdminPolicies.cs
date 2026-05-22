using TriPay.Data.Identity;

namespace TriPay.Admin.Authorization;

/// <summary>Authorization policy adları (= izin kodları).</summary>
public static class AdminPolicies
{
    public const string PanelAccess = AdminPermissions.PanelAccess;
    public const string DashboardView = AdminPermissions.DashboardView;
    public const string TransactionsView = AdminPermissions.TransactionsView;
    public const string ReportsView = AdminPermissions.ReportsView;
    public const string MerchantsView = AdminPermissions.MerchantsView;
    public const string MerchantsManage = AdminPermissions.MerchantsManage;
    public const string GatewaysView = AdminPermissions.GatewaysView;
    public const string GatewaysManage = AdminPermissions.GatewaysManage;
    public const string OutboxView = AdminPermissions.OutboxView;
    public const string OutboxManage = AdminPermissions.OutboxManage;
    public const string UsersManage = AdminPermissions.UsersManage;
    public const string RolesManage = AdminPermissions.RolesManage;
    public const string SystemView = AdminPermissions.SystemView;
    public const string SystemManage = AdminPermissions.SystemManage;
}

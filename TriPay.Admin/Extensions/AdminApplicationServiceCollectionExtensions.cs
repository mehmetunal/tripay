using TriPay.Admin.Application.Services;

namespace TriPay.Admin.Extensions;

public static class AdminApplicationServiceCollectionExtensions
{
    /// <summary>Admin uygulama servisleri (SOLID — controller → service → repository).</summary>
    public static IServiceCollection AddTriPayAdminApplication(this IServiceCollection services)
    {
        services.AddScoped<IAdminMerchantService, AdminMerchantService>();
        services.AddScoped<IAdminTransactionService, AdminTransactionService>();
        services.AddScoped<IAdminOutboxService, AdminOutboxService>();
        services.AddScoped<IAdminGatewayService, AdminGatewayService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminReportsService, AdminReportsService>();
        services.AddScoped<IAdminSystemService, AdminSystemService>();
        return services;
    }
}

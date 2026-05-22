using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TriPay.Data.Migrations;
using TriPay.Data.Persistence;
using TriPay.Data.Repositories;

namespace TriPay.Data.DependencyInjection;

/// <summary>TriPay.Data DI kayıtları.</summary>
public static class TriPayDataServiceCollectionExtensions
{
    /// <summary>MSSQL DbContext, repository ve FluentMigrator kaydı.</summary>
    public static IServiceCollection AddTriPayData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TriPay")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:TriPay veya DefaultConnection tanımlı olmalıdır.");

        var useInMemory = configuration.GetValue<bool>("TriPay:Database:UseInMemory");

        if (useInMemory)
        {
            services.AddDbContext<TriPayDbContext>(o => o.UseInMemoryDatabase("TriPay_InMemory"));
            services.AddHostedService<TriPayInMemoryDatabaseInitializer>();
        }
        else
        {
            services.AddDbContext<TriPayDbContext>(o =>
                o.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(3)));
        }

        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IGatewayMetadataRepository, GatewayMetadataRepository>();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSqlServer()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(InitialSchema).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }

    /// <summary>Bekleyen FluentMigrator migration'larını uygular.</summary>
    public static IServiceProvider RunTriPayMigrations(this IServiceProvider services)
    {
        if (services.GetRequiredService<IConfiguration>().GetValue<bool>("TriPay:Database:UseInMemory"))
            return services;

        using var scope = services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        if (!runner.HasMigrationsToApplyUp())
            return services;

        runner.MigrateUp();
        return services;
    }
}

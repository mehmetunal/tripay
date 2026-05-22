using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TriPay.Data.Entities;

namespace TriPay.Data.Persistence;

/// <summary>TriPay MSSQL veritabanı bağlamı (ödeme + Identity).</summary>
public class TriPayDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    /// <summary>DbContext oluşturur.</summary>
    public TriPayDbContext(DbContextOptions<TriPayDbContext> options)
        : base(options)
    {
    }

    /// <summary>Üye işyerleri.</summary>
    public DbSet<Merchant> Merchants => Set<Merchant>();

    /// <summary>Ödeme kanalları.</summary>
    public DbSet<PaymentGatewayRecord> PaymentGateways => Set<PaymentGatewayRecord>();

    /// <summary>Ödeme özet kayıtları.</summary>
    public DbSet<PaymentTransaction> Transactions => Set<PaymentTransaction>();

    /// <summary>İşlem adım logları.</summary>
    public DbSet<PaymentTransactionLog> TransactionLogs => Set<PaymentTransactionLog>();

    /// <summary>Outbox mesajları.</summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>Gateway teknik ayarları.</summary>
    public DbSet<GatewaySetting> GatewaySettings => Set<GatewaySetting>();

    /// <summary>Gateway hata kodu eşlemeleri.</summary>
    public DbSet<GatewayErrorMapping> GatewayErrorMappings => Set<GatewayErrorMapping>();

    /// <summary>Fluent API yapılandırması.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.DisplayName).HasMaxLength(128);
        });

        modelBuilder.Entity<Merchant>(e =>
        {
            e.ToTable("Merchants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.Property(x => x.ApiKey).HasMaxLength(128).IsRequired();
            e.Property(x => x.WebhookUrl).HasMaxLength(512);
        });

        modelBuilder.Entity<PaymentGatewayRecord>(e =>
        {
            e.ToTable("PaymentGateways");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<PaymentTransaction>(e =>
        {
            e.ToTable("Transactions");
            e.HasKey(x => x.Id);
            e.Property(x => x.OrderNumber).HasMaxLength(64).IsRequired();
            e.Property(x => x.ExternalTransactionId).HasMaxLength(128);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            e.Property(x => x.Status).HasMaxLength(32).IsRequired();
            e.Property(x => x.ResponseCode).HasMaxLength(16);
            e.Property(x => x.ResponseMessage).HasMaxLength(512);
            e.Property(x => x.ClientIp).HasMaxLength(45);
            e.Property(x => x.IdempotencyKey).HasMaxLength(256);
            e.HasIndex(x => new { x.MerchantId, x.OrderNumber }).IsUnique();
            e.HasIndex(x => x.ExternalTransactionId);
            e.HasOne(x => x.Merchant).WithMany().HasForeignKey(x => x.MerchantId);
            e.HasOne(x => x.PaymentGateway).WithMany().HasForeignKey(x => x.PaymentGatewayId);
        });

        modelBuilder.Entity<PaymentTransactionLog>(e =>
        {
            e.ToTable("TransactionLogs");
            e.HasKey(x => x.Id);
            e.Property(x => x.LogType).HasMaxLength(64).IsRequired();
            e.Property(x => x.Direction).HasMaxLength(16).IsRequired();
            e.Property(x => x.GatewayCode).HasMaxLength(32);
            e.Property(x => x.ErrorCode).HasMaxLength(64);
            e.Property(x => x.ErrorMessage).HasMaxLength(1024);
            e.HasIndex(x => new { x.TransactionId, x.CreatedAt });
            e.HasIndex(x => x.LogType);
            e.HasOne(x => x.Transaction).WithMany().HasForeignKey(x => x.TransactionId);
        });

        modelBuilder.Entity<OutboxMessage>(e =>
        {
            e.ToTable("OutboxMessages");
            e.HasKey(x => x.Id);
            e.Property(x => x.Payload).IsRequired();
            e.Property(x => x.RoutingKey).HasMaxLength(128).IsRequired();
            e.HasIndex(x => new { x.IsPublished, x.CreatedAt });
        });

        modelBuilder.Entity<GatewaySetting>(e =>
        {
            e.ToTable("GatewaySettings");
            e.HasKey(x => x.Id);
            e.Property(x => x.SettingKey).HasMaxLength(128).IsRequired();
            e.Property(x => x.SettingValue).HasMaxLength(1024).IsRequired();
            e.Property(x => x.Environment).HasMaxLength(16).IsRequired();
            e.HasIndex(x => new { x.PaymentGatewayId, x.SettingKey, x.Environment }).IsUnique();
        });

        modelBuilder.Entity<GatewayErrorMapping>(e =>
        {
            e.ToTable("GatewayErrorMappings");
            e.HasKey(x => x.Id);
            e.Property(x => x.ProviderErrorCode).HasMaxLength(64).IsRequired();
            e.Property(x => x.NormalizedCode).HasMaxLength(32);
            e.Property(x => x.UserMessage).HasMaxLength(512).IsRequired();
            e.Property(x => x.Locale).HasMaxLength(8).IsRequired();
            e.HasIndex(x => new { x.PaymentGatewayId, x.ProviderErrorCode, x.Locale }).IsUnique();
        });
    }
}

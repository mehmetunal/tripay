using FluentMigrator;

namespace TriPay.Data.Migrations;

/// <summary>Demo üye işyeri ve MVP ödeme kanalları seed verisi.</summary>
[Migration(202605220002)]
public sealed class SeedData : Migration
{
    private static readonly DateTime SeedUtc = new(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Seed kayıtlarını ekler.</summary>
    public override void Up()
    {
        Insert.IntoTable("Merchants").Row(new
        {
            Name = "TriPay Demo",
            ApiKey = "demo-api-key",
            WebhookUrl = (string?)null,
            IsActive = true,
            CreatedAt = SeedUtc
        });

        Insert.IntoTable("PaymentGateways").Row(new { Code = "VakifPays", DisplayName = "VakıfPayS", IsActive = true });
        Insert.IntoTable("PaymentGateways").Row(new { Code = "Iyzico", DisplayName = "iyzico", IsActive = true });
        Insert.IntoTable("PaymentGateways").Row(new { Code = "Vakifbank", DisplayName = "Vakıfbank", IsActive = true });
    }

    /// <summary>Seed kayıtlarını kaldırır.</summary>
    public override void Down()
    {
        Delete.FromTable("PaymentGateways").AllRows();
        Delete.FromTable("Merchants").AllRows();
    }
}

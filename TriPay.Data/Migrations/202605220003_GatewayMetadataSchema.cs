using FluentMigrator;

namespace TriPay.Data.Migrations;

/// <summary>Gateway ayarları ve hata eşleme tabloları.</summary>
[Migration(202605220003)]
public sealed class GatewayMetadataSchema : Migration
{
    /// <summary>Tabloları oluşturur.</summary>
    public override void Up()
    {
        Create.Table("GatewaySettings")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("PaymentGatewayId").AsInt32().NotNullable()
                .ForeignKey("FK_GatewaySettings_PaymentGateways", "PaymentGateways", "Id")
            .WithColumn("SettingKey").AsString(128).NotNullable()
            .WithColumn("SettingValue").AsString(1024).NotNullable()
            .WithColumn("Environment").AsString(16).NotNullable().WithDefaultValue("All")
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);

        Create.UniqueConstraint("UQ_GatewaySettings_Gateway_Key_Env")
            .OnTable("GatewaySettings")
            .Columns("PaymentGatewayId", "SettingKey", "Environment");

        Create.Table("GatewayErrorMappings")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("PaymentGatewayId").AsInt32().NotNullable()
                .ForeignKey("FK_GatewayErrorMappings_PaymentGateways", "PaymentGateways", "Id")
            .WithColumn("ProviderErrorCode").AsString(64).NotNullable()
            .WithColumn("NormalizedCode").AsString(32).Nullable()
            .WithColumn("UserMessage").AsString(512).NotNullable()
            .WithColumn("Locale").AsString(8).NotNullable().WithDefaultValue("tr")
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);

        Create.UniqueConstraint("UQ_GatewayErrorMappings_Gateway_Code_Locale")
            .OnTable("GatewayErrorMappings")
            .Columns("PaymentGatewayId", "ProviderErrorCode", "Locale");
    }

    /// <summary>Tabloları kaldırır.</summary>
    public override void Down()
    {
        Delete.Table("GatewayErrorMappings");
        Delete.Table("GatewaySettings");
    }
}

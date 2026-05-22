using FluentMigrator;

namespace TriPay.Data.Migrations;

/// <summary>TriPay MSSQL ilk şema (§9.3).</summary>
[Migration(202605220001)]
public sealed class InitialSchema : Migration
{
    /// <summary>Tabloları oluşturur.</summary>
    public override void Up()
    {
        Create.Table("Merchants")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(256).NotNullable()
            .WithColumn("ApiKey").AsString(128).NotNullable()
            .WithColumn("WebhookUrl").AsString(512).Nullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime2().NotNullable();

        Create.Table("PaymentGateways")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Code").AsString(64).NotNullable()
            .WithColumn("DisplayName").AsString(128).NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);

        Create.UniqueConstraint("UQ_PaymentGateways_Code").OnTable("PaymentGateways").Column("Code");

        Create.Table("Transactions")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("MerchantId").AsInt32().NotNullable()
                .ForeignKey("FK_Transactions_Merchants", "Merchants", "Id")
            .WithColumn("PaymentGatewayId").AsInt32().NotNullable()
                .ForeignKey("FK_Transactions_PaymentGateways", "PaymentGateways", "Id")
            .WithColumn("OrderNumber").AsString(64).NotNullable()
            .WithColumn("ExternalTransactionId").AsString(128).Nullable()
            .WithColumn("Amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Currency").AsString(3).NotNullable()
            .WithColumn("InstallmentCount").AsInt32().Nullable()
            .WithColumn("Status").AsString(32).NotNullable()
            .WithColumn("ResponseCode").AsString(16).Nullable()
            .WithColumn("ResponseMessage").AsString(512).Nullable()
            .WithColumn("ClientIp").AsString(45).Nullable()
            .WithColumn("IdempotencyKey").AsString(256).Nullable()
            .WithColumn("CreatedAt").AsDateTime2().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime2().NotNullable();

        Create.UniqueConstraint("UQ_Transactions_Merchant_Order")
            .OnTable("Transactions")
            .Columns("MerchantId", "OrderNumber");

        Create.Index("IX_Transactions_ExternalTransactionId")
            .OnTable("Transactions")
            .OnColumn("ExternalTransactionId").Ascending();

        Create.Table("TransactionLogs")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("TransactionId").AsInt32().NotNullable()
                .ForeignKey("FK_TransactionLogs_Transactions", "Transactions", "Id")
            .WithColumn("LogType").AsString(64).NotNullable()
            .WithColumn("Direction").AsString(16).NotNullable()
            .WithColumn("RequestPayload").AsString(int.MaxValue).Nullable()
            .WithColumn("ResponsePayload").AsString(int.MaxValue).Nullable()
            .WithColumn("HttpStatusCode").AsInt32().Nullable()
            .WithColumn("GatewayCode").AsString(32).Nullable()
            .WithColumn("ErrorCode").AsString(64).Nullable()
            .WithColumn("ErrorMessage").AsString(1024).Nullable()
            .WithColumn("DurationMs").AsInt32().Nullable()
            .WithColumn("CreatedAt").AsDateTime2().NotNullable();

        Create.Index("IX_TransactionLogs_TransactionId_CreatedAt")
            .OnTable("TransactionLogs")
            .OnColumn("TransactionId").Ascending()
            .OnColumn("CreatedAt").Ascending();

        Create.Index("IX_TransactionLogs_LogType")
            .OnTable("TransactionLogs")
            .OnColumn("LogType").Ascending();

        Create.Table("OutboxMessages")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("TransactionId").AsInt32().NotNullable()
                .ForeignKey("FK_OutboxMessages_Transactions", "Transactions", "Id")
            .WithColumn("Payload").AsString(int.MaxValue).NotNullable()
            .WithColumn("RoutingKey").AsString(128).NotNullable()
            .WithColumn("IsPublished").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("PublishedAt").AsDateTime2().Nullable()
            .WithColumn("RetryCount").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("CreatedAt").AsDateTime2().NotNullable();

        Create.Index("IX_OutboxMessages_IsPublished_CreatedAt")
            .OnTable("OutboxMessages")
            .OnColumn("IsPublished").Ascending()
            .OnColumn("CreatedAt").Ascending();
    }

    /// <summary>Tabloları kaldırır.</summary>
    public override void Down()
    {
        Delete.Table("OutboxMessages");
        Delete.Table("TransactionLogs");
        Delete.Table("Transactions");
        Delete.Table("PaymentGateways");
        Delete.Table("Merchants");
    }
}

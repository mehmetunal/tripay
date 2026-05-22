using FluentMigrator;

namespace TriPay.Data.Migrations;

/// <summary>ASP.NET Core Identity tabloları (FluentMigrator — EF migration değil).</summary>
[Migration(202605220010)]
public sealed class IdentitySchema : Migration
{
    /// <summary>Identity şemasını oluşturur.</summary>
    public override void Up()
    {
        Create.Table("AspNetRoles")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(256).Nullable()
            .WithColumn("NormalizedName").AsString(256).Nullable()
            .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable();

        Create.Index("IX_AspNetRoles_NormalizedName")
            .OnTable("AspNetRoles")
            .OnColumn("NormalizedName")
            .Ascending()
            .WithOptions().Unique();

        Create.Table("AspNetUsers")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("DisplayName").AsString(128).Nullable()
            .WithColumn("UserName").AsString(256).Nullable()
            .WithColumn("NormalizedUserName").AsString(256).Nullable()
            .WithColumn("Email").AsString(256).Nullable()
            .WithColumn("NormalizedEmail").AsString(256).Nullable()
            .WithColumn("EmailConfirmed").AsBoolean().NotNullable()
            .WithColumn("PasswordHash").AsString(int.MaxValue).Nullable()
            .WithColumn("SecurityStamp").AsString(int.MaxValue).Nullable()
            .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable()
            .WithColumn("PhoneNumber").AsString(int.MaxValue).Nullable()
            .WithColumn("PhoneNumberConfirmed").AsBoolean().NotNullable()
            .WithColumn("TwoFactorEnabled").AsBoolean().NotNullable()
            .WithColumn("LockoutEnd").AsDateTimeOffset().Nullable()
            .WithColumn("LockoutEnabled").AsBoolean().NotNullable()
            .WithColumn("AccessFailedCount").AsInt32().NotNullable();

        Create.Index("IX_AspNetUsers_NormalizedUserName")
            .OnTable("AspNetUsers")
            .OnColumn("NormalizedUserName")
            .Ascending()
            .WithOptions().Unique();

        Create.Index("IX_AspNetUsers_NormalizedEmail")
            .OnTable("AspNetUsers")
            .OnColumn("NormalizedEmail")
            .Ascending();

        Create.Table("AspNetRoleClaims")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("RoleId").AsInt32().NotNullable()
            .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
            .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable();

        Create.ForeignKey("FK_AspNetRoleClaims_AspNetRoles")
            .FromTable("AspNetRoleClaims").ForeignColumn("RoleId")
            .ToTable("AspNetRoles").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Table("AspNetUserClaims")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
            .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable();

        Create.ForeignKey("FK_AspNetUserClaims_AspNetUsers")
            .FromTable("AspNetUserClaims").ForeignColumn("UserId")
            .ToTable("AspNetUsers").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Table("AspNetUserLogins")
            .WithColumn("LoginProvider").AsString(128).NotNullable()
            .WithColumn("ProviderKey").AsString(128).NotNullable()
            .WithColumn("ProviderDisplayName").AsString(int.MaxValue).Nullable()
            .WithColumn("UserId").AsInt32().NotNullable();

        Create.PrimaryKey("PK_AspNetUserLogins")
            .OnTable("AspNetUserLogins")
            .Columns("LoginProvider", "ProviderKey");

        Create.ForeignKey("FK_AspNetUserLogins_AspNetUsers")
            .FromTable("AspNetUserLogins").ForeignColumn("UserId")
            .ToTable("AspNetUsers").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Table("AspNetUserRoles")
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("RoleId").AsInt32().NotNullable();

        Create.PrimaryKey("PK_AspNetUserRoles")
            .OnTable("AspNetUserRoles")
            .Columns("UserId", "RoleId");

        Create.ForeignKey("FK_AspNetUserRoles_AspNetUsers")
            .FromTable("AspNetUserRoles").ForeignColumn("UserId")
            .ToTable("AspNetUsers").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("FK_AspNetUserRoles_AspNetRoles")
            .FromTable("AspNetUserRoles").ForeignColumn("RoleId")
            .ToTable("AspNetRoles").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Table("AspNetUserTokens")
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("LoginProvider").AsString(128).NotNullable()
            .WithColumn("Name").AsString(128).NotNullable()
            .WithColumn("Value").AsString(int.MaxValue).Nullable();

        Create.PrimaryKey("PK_AspNetUserTokens")
            .OnTable("AspNetUserTokens")
            .Columns("UserId", "LoginProvider", "Name");

        Create.ForeignKey("FK_AspNetUserTokens_AspNetUsers")
            .FromTable("AspNetUserTokens").ForeignColumn("UserId")
            .ToTable("AspNetUsers").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);
    }

    /// <summary>Identity tablolarını kaldırır.</summary>
    public override void Down()
    {
        Delete.Table("AspNetUserTokens");
        Delete.Table("AspNetUserRoles");
        Delete.Table("AspNetUserLogins");
        Delete.Table("AspNetUserClaims");
        Delete.Table("AspNetRoleClaims");
        Delete.Table("AspNetUsers");
        Delete.Table("AspNetRoles");
    }
}

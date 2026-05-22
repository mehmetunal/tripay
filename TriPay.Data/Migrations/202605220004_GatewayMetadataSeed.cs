using FluentMigrator;

namespace TriPay.Data.Migrations;

/// <summary>Vakıfbank gateway ayarları ve hata kodları seed verisi.</summary>
[Migration(202605220004)]
public sealed class GatewayMetadataSeed : Migration
{
    /// <summary>Seed kayıtlarını ekler.</summary>
    public override void Up()
    {
        Execute.Sql("""
            DECLARE @GatewayId INT = (SELECT Id FROM PaymentGateways WHERE Code = 'Vakifbank');

            INSERT INTO GatewaySettings (PaymentGatewayId, SettingKey, SettingValue, Environment, IsActive) VALUES
            (@GatewayId, 'EnrollmentUrl', 'https://3dsecuretest.vakifbank.com.tr/MPIAPI/MPI_Enrollment.aspx', 'Test', 1),
            (@GatewayId, 'VerifyUrl', 'https://onlineodemetest.vakifbank.com.tr/VposService/v3/Vposreq.aspx', 'Test', 1),
            (@GatewayId, 'EnrollmentUrl', 'https://3dsecure.vakifbank.com.tr/MPIAPI/MPI_Enrollment.aspx', 'Production', 1),
            (@GatewayId, 'VerifyUrl', 'https://onlineodeme.vakifbank.com.tr/VposService/v3/Vposreq.aspx', 'Production', 1),
            (@GatewayId, 'ResultCodeSuccess', '0000', 'All', 1),
            (@GatewayId, 'ThreeDsStatusEnrolled', 'Y', 'All', 1),
            (@GatewayId, 'ThreeDsStatusAttempt', 'A', 'All', 1),
            (@GatewayId, 'ThreeDsStatusNotEnrolled', 'N', 'All', 1),
            (@GatewayId, 'ErrorCodeIssuerException', '1001', 'All', 1),
            (@GatewayId, 'NotEnrolledUserMessage', N'Kartınız 3D Secure ile doğrulanamadı veya bankanız işlemi kabul etmedi. Lütfen farklı bir kart deneyin veya kartınızı veren banka ile iletişime geçin.', 'All', 1);

            INSERT INTO GatewayErrorMappings (PaymentGatewayId, ProviderErrorCode, NormalizedCode, UserMessage, Locale, IsActive) VALUES
            (@GatewayId, '0000', '00', N'Başarılı', 'tr', 1),
            (@GatewayId, '0005', '05', N'İşlem onaylanmadı', 'tr', 1),
            (@GatewayId, '0014', '14', N'Geçersiz kart numarası', 'tr', 1),
            (@GatewayId, '0033', '33', N'Süresi bitmiş kart', 'tr', 1),
            (@GatewayId, '0051', '51', N'Limit yetersiz', 'tr', 1),
            (@GatewayId, '0055', '55', N'Hatalı kart şifresi', 'tr', 1),
            (@GatewayId, '0580', '80', N'Hatalı 3D güvenlik bilgisi', 'tr', 1),
            (@GatewayId, '0581', '81', N'ECI veya CAVV bilgisi eksik', 'tr', 1),
            (@GatewayId, '1001', '1001', N'Kart 3D Secure programına kayıtlı değil', 'tr', 1),
            (@GatewayId, '1006', '1006', N'Bu işlem daha önce başarılı gerçekleştirilmiş', 'tr', 1),
            (@GatewayId, '1007', '1007', N'Referans işlem alınamadı', 'tr', 1),
            (@GatewayId, '1050', '1050', N'CVV hatalı', 'tr', 1),
            (@GatewayId, '1051', '1051', N'Kredi kartı numarası hatalı', 'tr', 1),
            (@GatewayId, '1052', '1052', N'Kart son kullanma tarihi hatalı', 'tr', 1),
            (@GatewayId, '5001', '5001', N'İş yeri şifresi yanlış', 'tr', 1),
            (@GatewayId, '5002', '5002', N'İş yeri aktif değil', 'tr', 1);
            """);
    }

    /// <summary>Seed kayıtlarını kaldırır.</summary>
    public override void Down()
    {
        Execute.Sql("""
            DELETE FROM GatewayErrorMappings WHERE PaymentGatewayId = (SELECT Id FROM PaymentGateways WHERE Code = 'Vakifbank');
            DELETE FROM GatewaySettings WHERE PaymentGatewayId = (SELECT Id FROM PaymentGateways WHERE Code = 'Vakifbank');
            """);
    }
}

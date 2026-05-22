using Microsoft.EntityFrameworkCore;
using TriPay.Data.Entities;

namespace TriPay.Data.Persistence;

/// <summary>InMemory ve test ortamları için seed yardımcısı.</summary>
public static class TriPayDbSeed
{
    /// <summary>Şema yoksa oluşturur ve demo veriyi ekler.</summary>
    public static async Task EnsureDemoDataAsync(TriPayDbContext db, CancellationToken cancellationToken = default)
    {
        if (db.Database.IsRelational())
            return;

        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.Merchants.AnyAsync(cancellationToken)
            || await db.GatewaySettings.AnyAsync(cancellationToken))
            return;

        var now = DateTime.UtcNow;
        db.Merchants.Add(new Merchant
        {
            Name = "TriPay Demo",
            ApiKey = "demo-api-key",
            IsActive = true,
            CreatedAt = now
        });

        db.PaymentGateways.AddRange(
            new PaymentGatewayRecord { Code = "VakifPays", DisplayName = "VakıfPayS", IsActive = true },
            new PaymentGatewayRecord { Code = "Iyzico", DisplayName = "iyzico", IsActive = true },
            new PaymentGatewayRecord { Code = "Vakifbank", DisplayName = "Vakıfbank", IsActive = true });

        await db.SaveChangesAsync(cancellationToken);

        var vakifbankId = await db.PaymentGateways.Where(g => g.Code == "Vakifbank").Select(g => g.Id).FirstAsync(cancellationToken);
        db.GatewaySettings.AddRange(
            new GatewaySetting { PaymentGatewayId = vakifbankId, SettingKey = "EnrollmentUrl", SettingValue = "https://3dsecuretest.vakifbank.com.tr/MPIAPI/MPI_Enrollment.aspx", Environment = "Test" },
            new GatewaySetting { PaymentGatewayId = vakifbankId, SettingKey = "VerifyUrl", SettingValue = "https://onlineodemetest.vakifbank.com.tr/VposService/v3/Vposreq.aspx", Environment = "Test" },
            new GatewaySetting { PaymentGatewayId = vakifbankId, SettingKey = "ResultCodeSuccess", SettingValue = "0000", Environment = "All" },
            new GatewaySetting { PaymentGatewayId = vakifbankId, SettingKey = "ThreeDsStatusEnrolled", SettingValue = "Y", Environment = "All" });
        db.GatewayErrorMappings.Add(new GatewayErrorMapping
        {
            PaymentGatewayId = vakifbankId,
            ProviderErrorCode = "0051",
            UserMessage = "Limit yetersiz",
            Locale = "tr"
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}

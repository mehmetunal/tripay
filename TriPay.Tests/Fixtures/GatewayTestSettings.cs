namespace TriPay.Tests.Fixtures;

/// <summary>Tüm gateway testleri için ortak sahte yapılandırma anahtarları.</summary>
public static class GatewayTestSettings
{
    /// <summary>Çoğu banka ve ödeme kuruluşu için geçerli minimal ayar seti.</summary>
    public static Dictionary<string, string> Standard()
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["MerchantId"] = "TEST-MERCHANT",
            ["Merchant"] = "TEST-MERCHANT",
            ["Username"] = "test-user",
            ["MerchantUser"] = "test-user",
            ["Password"] = "test-password",
            ["MerchantPassword"] = "test-password",
            ["StoreKey"] = "test-store-key",
            ["MerchantStorekey"] = "test-store-key",
            ["TerminalNo"] = "T001",
            ["TerminalId"] = "T001",
            ["ProvPassword"] = "test-prov-password",
            ["ApiKey"] = "test-api-key",
            ["SecretKey"] = "test-secret-key",
            ["MerchantKey"] = "test-merchant-key",
            ["AppId"] = "test-app-id",
            ["AppSecret"] = "test-app-secret",
            ["MemberId"] = "12345",
            ["UserCode"] = "test-user",
            ["Email"] = "test@test.com",
            ["PosnetId"] = "test-posnet",
            ["Guid"] = "test-guid-0000",
            ["ClientCode"] = "TEST-CLIENT",
            ["ClientUsername"] = "test-client-user",
            ["ClientPassword"] = "test-client-pass",
            ["DealerCode"] = "TEST-DEALER",
            ["Sx"] = "TEST-SX"
        };

    /// <summary>Tami JWK imza test şifresi (kid|key).</summary>
    public static Dictionary<string, string> Tami()
        => new(Standard())
        {
            ["MerchantPassword"] = "kid|dGVzdA=="
        };

    /// <summary>Ahlpay üye kimliği sayısal olmalıdır.</summary>
    public static Dictionary<string, string> Ahlpay()
        => new(Standard())
        {
            ["MerchantId"] = "12345",
            ["MemberId"] = "12345"
        };

    /// <summary>Vakıfbank MPI/VPOS test ayarları.</summary>
    public static Dictionary<string, string> Vakifbank()
        => new(Standard())
        {
            ["EnrollmentUrl"] = "https://3dsecuretest.vakifbank.com.tr/MPIAPI/MPI_Enrollment.aspx",
            ["VerifyUrl"] = "https://onlineodemetest.vakifbank.com.tr/VposService/v3/Vposreq.aspx",
            ["ResultCodeSuccess"] = "0000",
            ["ThreeDsStatusEnrolled"] = "Y",
            ["ThreeDsStatusAttempt"] = "A",
            ["ThreeDsStatusNotEnrolled"] = "N",
            ["ErrorCodeIssuerException"] = "1001",
            ["InstallmentCounts"] = "3,6",
            ["BinPrefixes"] = "493841"
        };

    /// <summary>API v2 protokol test ayarları.</summary>
    public static Dictionary<string, string> ApiV2()
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["Merchant"] = "10009011",
            ["MerchantUser"] = "apitest@test.com",
            ["MerchantPassword"] = "Api.123.1234"
        };

    /// <summary>CCPayment protokol test ayarları.</summary>
    public static Dictionary<string, string> CcPayment()
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["ApiKey"] = "test-app-id",
            ["SecretKey"] = "test-app-secret",
            ["MerchantKey"] = "test-merchant-key"
        };

    /// <summary>Iyzico test ayarları.</summary>
    public static Dictionary<string, string> Iyzico()
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["ApiKey"] = "sandbox-key",
            ["SecretKey"] = "sandbox-secret"
        };
}

namespace TriPay.Services.Providers.Vakifbank;

/// <summary>Vakıfbank provider sabitleri (Trimango StringConstants karşılığı).</summary>
public static class VakifbankConstants
{
    /// <summary>VPOS başarılı sonuç kodu.</summary>
    public const string ResultCodeSuccess = "0000";

    /// <summary>3D kayıtlı kart durumu.</summary>
    public const string ThreeDsStatusEnrolled = "Y";

    /// <summary>3D deneme (attempt) durumu.</summary>
    public const string ThreeDsStatusAttempt = "A";

    /// <summary>3D kayıtsız kart durumu.</summary>
    public const string ThreeDsStatusNotEnrolled = "N";

    /// <summary>Issuer exception hata kodu (kayıtsız kart senaryosu).</summary>
    public const string ErrorCodeIssuerException = "1001";

    /// <summary>Varsayılan kart marka kodu.</summary>
    public const string DefaultBrandCode = "100";

    /// <summary>Varsayılan ISO para birimi kodu (TRY).</summary>
    public const string DefaultCurrencyCode = "949";

    /// <summary>Test MPI enrollment URL.</summary>
    public const string EnrollmentUrlTest = "https://3dsecuretest.vakifbank.com.tr/MPIAPI/MPI_Enrollment.aspx";

    /// <summary>Prod MPI enrollment URL.</summary>
    public const string EnrollmentUrlProd = "https://3dsecure.vakifbank.com.tr/MPIAPI/MPI_Enrollment.aspx";

    /// <summary>Test VPOS URL.</summary>
    public const string VerifyUrlTest = "https://onlineodemetest.vakifbank.com.tr/VposService/v3/Vposreq.aspx";

    /// <summary>Prod VPOS URL.</summary>
    public const string VerifyUrlProd = "https://onlineodeme.vakifbank.com.tr/VposService/v3/Vposreq.aspx";
}

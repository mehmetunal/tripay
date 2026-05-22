namespace TriPay.Core.Gateways;

/// <summary>PaymentGateways için ortak ayar anahtarları (DB <c>GatewaySettings.SettingKey</c>).</summary>
public static class GatewaySettingKeys
{
    /// <summary>MPI enrollment endpoint URL.</summary>
    public const string EnrollmentUrl = "EnrollmentUrl";

    /// <summary>VPOS verify/sale endpoint URL.</summary>
    public const string VerifyUrl = "VerifyUrl";

    /// <summary>VPOS başarılı sonuç kodu.</summary>
    public const string ResultCodeSuccess = "ResultCodeSuccess";

    /// <summary>3D kayıtlı kart durumu.</summary>
    public const string ThreeDsStatusEnrolled = "ThreeDsStatusEnrolled";

    /// <summary>3D attempt durumu.</summary>
    public const string ThreeDsStatusAttempt = "ThreeDsStatusAttempt";

    /// <summary>3D kayıtsız kart durumu.</summary>
    public const string ThreeDsStatusNotEnrolled = "ThreeDsStatusNotEnrolled";

    /// <summary>Issuer exception (kayıtsız kart) hata kodu.</summary>
    public const string ErrorCodeIssuerException = "ErrorCodeIssuerException";

    /// <summary>Kayıtsız kart kullanıcı mesajı.</summary>
    public const string NotEnrolledUserMessage = "NotEnrolledUserMessage";
}

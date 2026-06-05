namespace TriPay.Tests.Fixtures;

/// <summary>Gateway provider protokol grupları (ortak test davranışı için).</summary>
public enum GatewayProtocolKind
{
    /// <summary>API v2 form POST protokolü (VakıfPayS, Paratika, Payten MSU, ZiraatPay).</summary>
    ApiV2,

    /// <summary>Nestpay/EST XML protokolü.</summary>
    Nestpay,

    /// <summary>CCPayment REST protokolü (Sipay, QNBpay vb.).</summary>
    CcPayment,

    /// <summary>Vakıfbank MPI + VPOS.</summary>
    Vakifbank,

    /// <summary>Iyzico REST API.</summary>
    Iyzico,

    /// <summary>mdStatus tabanlı banka 3D callback.</summary>
    MdStatusBank,

    /// <summary>ParamPos callback.</summary>
    ParamPos,

    /// <summary>Moka callback.</summary>
    Moka,

    /// <summary>PayNKolay callback.</summary>
    PayNKolay,

    /// <summary>Paynet callback.</summary>
    Paynet,

    /// <summary>Ahlpay callback.</summary>
    Ahlpay,

    /// <summary>Tami JSON API callback.</summary>
    Tami
}

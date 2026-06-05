namespace TriPay.Services.Providers.CcPayment;

/// <summary>CCPayment kanalı için test ve canlı API kök adresleri.</summary>
public sealed record CcPaymentEndpointConfig(string TestBaseUrl, string LiveBaseUrl)
{
    /// <summary>Test modunda kök URL döndürür.</summary>
    public string Resolve(bool isTestMode) => isTestMode ? TestBaseUrl : LiveBaseUrl;
}

/// <summary>Ödeme kuruluşu bazlı CCPayment endpoint tanımları.</summary>
public static class CcPaymentEndpoints
{
    /// <summary>Sipay endpointleri.</summary>
    public static readonly CcPaymentEndpointConfig Sipay = new(
        "https://provisioning.sipay.com.tr/ccpayment",
        "https://app.sipay.com.tr/ccpayment");

    /// <summary>QNBpay endpointleri.</summary>
    public static readonly CcPaymentEndpointConfig QNBpay = new(
        "https://test.qnbpay.com.tr/ccpayment",
        "https://portal.qnbpay.com.tr/ccpayment");

    /// <summary>PayBull endpointleri.</summary>
    public static readonly CcPaymentEndpointConfig PayBull = new(
        "https://test.paybull.com/ccpayment",
        "https://app.paybull.com/ccpayment");

    /// <summary>Parolapara endpointleri.</summary>
    public static readonly CcPaymentEndpointConfig Parolapara = new(
        "https://testccpayment.parolapara.com/ccpayment",
        "https://ccpayment.parolapara.com/ccpayment");

    /// <summary>IQmoney endpointleri.</summary>
    public static readonly CcPaymentEndpointConfig IQmoney = new(
        "https://provisioning.iqmoneytr.com/ccpayment",
        "https://app.iqmoneytr.com/ccpayment");

    /// <summary>Vepara endpointleri.</summary>
    public static readonly CcPaymentEndpointConfig Vepara = new(
        "https://test.vepara.com.tr/ccpayment",
        "https://app.vepara.com.tr/ccpayment");

    /// <summary>HalkÖde endpointleri.</summary>
    public static readonly CcPaymentEndpointConfig HalkOde = new(
        "https://testapp.halkode.com.tr/ccpayment",
        "https://app.halkode.com.tr/ccpayment");
}

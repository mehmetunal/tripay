namespace TriPay.Core.Gateways;

/// <summary>
/// Kayıtlı ödeme gateway kodları. Magic string kullanılmaz; Factory ve DTO atamalarında bu sabitler kullanılır.
/// </summary>
public static class PaymentGatewayNames
{
    /// <summary>Varsayılan gateway kodu (yapılandırmadaki <c>DefaultGateway</c> ile uyumlu olmalıdır).</summary>
    public const string Default = VakifPays;

    /// <summary>Akbank sanal POS kanal kodu.</summary>
    public const string Akbank = "Akbank";

    /// <summary>Akbank Nestpay kanal kodu.</summary>
    public const string AkbankNestpay = "AkbankNestpay";

    /// <summary>Alternatif Bank kanal kodu.</summary>
    public const string AlternatifBank = "AlternatifBank";

    /// <summary>Anadolubank kanal kodu.</summary>
    public const string Anadolubank = "Anadolubank";

    /// <summary>Denizbank kanal kodu.</summary>
    public const string Denizbank = "Denizbank";

    /// <summary>QNB Finansbank kanal kodu.</summary>
    public const string QNBFinansbank = "QNBFinansbank";

    /// <summary>Finansbank Nestpay kanal kodu.</summary>
    public const string FinansbankNestpay = "FinansbankNestpay";

    /// <summary>Garanti BBVA kanal kodu.</summary>
    public const string Garanti = "Garanti";

    /// <summary>Halkbank kanal kodu.</summary>
    public const string Halkbank = "Halkbank";

    /// <summary>ING Bank kanal kodu.</summary>
    public const string ING = "ING";

    /// <summary>İş Bankası kanal kodu.</summary>
    public const string IsBankasi = "IsBankasi";

    /// <summary>Şekerbank kanal kodu.</summary>
    public const string Sekerbank = "Sekerbank";

    /// <summary>Türk Ekonomi Bankası kanal kodu.</summary>
    public const string TurkEkonomiBankasi = "TurkEkonomiBankasi";

    /// <summary>Türkiye Finans kanal kodu.</summary>
    public const string TurkiyeFinans = "TurkiyeFinans";

    /// <summary>Vakıfbank sanal POS kanal kodu.</summary>
    public const string Vakifbank = "Vakifbank";

    /// <summary>Yapı Kredi kanal kodu.</summary>
    public const string YapiKredi = "YapiKredi";

    /// <summary>Ziraat Bankası kanal kodu.</summary>
    public const string Ziraat = "Ziraat";

    /// <summary>Kuveyt Türk kanal kodu.</summary>
    public const string KuveytTurk = "KuveytTurk";

    /// <summary>Vakıf Katılım kanal kodu.</summary>
    public const string VakifKatilim = "VakifKatilim";

    /// <summary>Cardplus ödeme kuruluşu kanal kodu.</summary>
    public const string Cardplus = "Cardplus";

    /// <summary>Paratika kanal kodu.</summary>
    public const string Paratika = "Paratika";

    /// <summary>Payten MSU kanal kodu.</summary>
    public const string PaytenMsu = "PaytenMsu";

    /// <summary>Iyzico ödeme kuruluşu kanal kodu.</summary>
    public const string Iyzico = "Iyzico";

    /// <summary>Sipay kanal kodu.</summary>
    public const string Sipay = "Sipay";

    /// <summary>QNBpay kanal kodu.</summary>
    public const string QNBpay = "QNBpay";

    /// <summary>ParamPos kanal kodu.</summary>
    public const string ParamPos = "ParamPos";

    /// <summary>PayBull kanal kodu.</summary>
    public const string PayBull = "PayBull";

    /// <summary>Parolapara kanal kodu.</summary>
    public const string Parolapara = "Parolapara";

    /// <summary>IQmoney kanal kodu.</summary>
    public const string IQmoney = "IQmoney";

    /// <summary>Ahlpay kanal kodu.</summary>
    public const string Ahlpay = "Ahlpay";

    /// <summary>Moka kanal kodu.</summary>
    public const string Moka = "Moka";

    /// <summary>Vepara kanal kodu.</summary>
    public const string Vepara = "Vepara";

    /// <summary>ZiraatPay kanal kodu.</summary>
    public const string ZiraatPay = "ZiraatPay";

    /// <summary>VakıfPayS kanal kodu.</summary>
    public const string VakifPays = "VakifPays";

    /// <summary>Tami kanal kodu.</summary>
    public const string Tami = "Tami";

    /// <summary>HalkÖde kanal kodu.</summary>
    public const string HalkOde = "HalkOde";

    /// <summary>PayNKolay kanal kodu.</summary>
    public const string PayNKolay = "PayNKolay";

    /// <summary>Paynet kanal kodu.</summary>
    public const string Paynet = "Paynet";

    /// <summary>PayTR kanal kodu (backlog).</summary>
    public const string PayTR = "PayTR";
}

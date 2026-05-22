namespace TriPay.Services;

/// <summary>
/// Kayıtlı ödeme gateway kodları. Magic string kullanılmaz; Factory ve DTO atamalarında bu sabitler kullanılır.
/// </summary>
public static class PaymentGatewayNames
{
    /// <summary>Varsayılan gateway (şu an tek aktif kanal).</summary>
    public const string Default = VakifPays;

    // Bankalar
    public const string Akbank = "Akbank";
    public const string AkbankNestpay = "AkbankNestpay";
    public const string AlternatifBank = "AlternatifBank";
    public const string Anadolubank = "Anadolubank";
    public const string Denizbank = "Denizbank";
    public const string QNBFinansbank = "QNBFinansbank";
    public const string FinansbankNestpay = "FinansbankNestpay";
    public const string Garanti = "Garanti";
    public const string Halkbank = "Halkbank";
    public const string ING = "ING";
    public const string IsBankasi = "IsBankasi";
    public const string Sekerbank = "Sekerbank";
    public const string TurkEkonomiBankasi = "TurkEkonomiBankasi";
    public const string TurkiyeFinans = "TurkiyeFinans";
    public const string Vakifbank = "Vakifbank";
    public const string YapiKredi = "YapiKredi";
    public const string Ziraat = "Ziraat";
    public const string KuveytTurk = "KuveytTurk";
    public const string VakifKatilim = "VakifKatilim";

    // Ödeme kuruluşları
    public const string Cardplus = "Cardplus";
    public const string Paratika = "Paratika";
    public const string PaytenMsu = "PaytenMsu";
    public const string Iyzico = "Iyzico";
    public const string Sipay = "Sipay";
    public const string QNBpay = "QNBpay";
    public const string ParamPos = "ParamPos";
    public const string PayBull = "PayBull";
    public const string Parolapara = "Parolapara";
    public const string IQmoney = "IQmoney";
    public const string Ahlpay = "Ahlpay";
    public const string Moka = "Moka";
    public const string Vepara = "Vepara";
    public const string ZiraatPay = "ZiraatPay";
    public const string VakifPays = "VakifPays";
    public const string Tami = "Tami";
    public const string HalkOde = "HalkOde";
    public const string PayNKolay = "PayNKolay";
    public const string Paynet = "Paynet";
    public const string PayTR = "PayTR";
}

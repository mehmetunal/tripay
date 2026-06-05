namespace TriPay.Services.Providers.Nestpay;

/// <summary>Banka bazlı Nestpay endpoint yapılandırmaları.</summary>
public static class NestpayEndpoints
{
    /// <summary>Akbank Nestpay canlı/test adresleri.</summary>
    public static readonly NestpayEndpointConfig AkbankNestpay = new(
        "https://www.sanalakpos.com/fim/api",
        "https://www.sanalakpos.com/fim/est3Dgate");

    /// <summary>Alternatif Bank canlı adresleri.</summary>
    public static readonly NestpayEndpointConfig AlternatifBank = new(
        "https://sanalpos.abank.com.tr/fim/api",
        "https://sanalpos.abank.com.tr/fim/est3Dgate");

    /// <summary>Anadolubank canlı adresleri.</summary>
    public static readonly NestpayEndpointConfig Anadolubank = new(
        "https://anadolusanalpos.est.com.tr/fim/api",
        "https://anadolusanalpos.est.com.tr/fim/est3Dgate");

    /// <summary>Cardplus canlı adresleri.</summary>
    public static readonly NestpayEndpointConfig Cardplus = new(
        "https://sanalpos.card-plus.net/fim/api",
        "https://sanalpos.card-plus.net/fim/est3Dgate");

    /// <summary>Halkbank canlı adresleri.</summary>
    public static readonly NestpayEndpointConfig Halkbank = new(
        "https://sanalpos.halkbank.com.tr/fim/api",
        "https://sanalpos.halkbank.com.tr/fim/est3Dgate");

    /// <summary>ING Bank canlı adresleri.</summary>
    public static readonly NestpayEndpointConfig ING = new(
        "https://sanalpos.ingbank.com.tr/fim/api",
        "https://sanalpos.ingbank.com.tr/fim/est3Dgate");

    /// <summary>İş Bankası canlı ve test adresleri.</summary>
    public static readonly NestpayEndpointConfig IsBankasi = new(
        "https://sanalpos.isbank.com.tr/fim/api",
        "https://sanalpos.isbank.com.tr/fim/est3Dgate",
        "https://istest.asseco-see.com.tr/fim/api",
        "https://istest.asseco-see.com.tr/fim/est3Dgate");

    /// <summary>QNB Finansbank Nestpay (eski Finansbank) canlı adresleri.</summary>
    public static readonly NestpayEndpointConfig FinansbankNestpay = new(
        "https://www.fbwebpos.com/fim/api",
        "https://www.fbwebpos.com/fim/est3Dgate");

    /// <summary>Şekerbank canlı adresleri.</summary>
    public static readonly NestpayEndpointConfig Sekerbank = new(
        "https://sanalpos.sekerbank.com.tr/fim/api",
        "https://sanalpos.sekerbank.com.tr/fim/est3Dgate");

    /// <summary>TEB canlı adresleri.</summary>
    public static readonly NestpayEndpointConfig TurkEkonomiBankasi = new(
        "https://sanalpos.teb.com.tr/fim/api",
        "https://sanalpos.teb.com.tr/fim/est3Dgate");

    /// <summary>Türkiye Finans canlı adresleri.</summary>
    public static readonly NestpayEndpointConfig TurkiyeFinans = new(
        "https://sanalpos.turkiyefinans.com.tr/fim/api",
        "https://sanalpos.turkiyefinans.com.tr/fim/est3Dgate");

    /// <summary>Ziraat Bankası canlı ve test adresleri.</summary>
    public static readonly NestpayEndpointConfig Ziraat = new(
        "https://sanalpos2.ziraatbank.com.tr/fim/api",
        "https://sanalpos2.ziraatbank.com.tr/fim/est3Dgate",
        "https://torus-stage-ziraat.asseco-see.com.tr/fim/api",
        "https://torus-stage-ziraat.asseco-see.com.tr/fim/est3Dgate");
}

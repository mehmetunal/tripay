namespace TriPay.Services.Providers.Protocols.ApiV2;

/// <summary>API v2 protokolünü kullanan kanalların endpoint tanımları.</summary>
public static class ApiV2Endpoints
{
    /// <summary>Paratika endpoint yapılandırması.</summary>
    public static readonly ApiV2EndpointConfig Paratika = new()
    {
        ApiUrlTest = "https://entegrasyon.paratika.com.tr/paratika/api/v2",
        ApiUrlLive = "https://vpos.paratika.com.tr/paratika/api/v2",
        Sale3DUrlTestTemplate = "https://entegrasyon.paratika.com.tr/paratika/api/v2/post/sale3d/{0}",
        Sale3DUrlLiveTemplate = "https://vpos.paratika.com.tr/paratika/api/v2/post/sale3d/{0}",
        FraudMetrixOrgId = "6bmm5c3v"
    };

    /// <summary>Payten MSU endpoint yapılandırması.</summary>
    public static readonly ApiV2EndpointConfig PaytenMsu = new()
    {
        ApiUrlTest = "https://entegrasyon.asseco-see.com.tr/msu/api/v2",
        ApiUrlLive = "https://merchantsafeunipay.com/msu/api/v2",
        Sale3DUrlTestTemplate = "https://entegrasyon.asseco-see.com.tr/msu/api/v2/post/sale3d/{0}",
        Sale3DUrlLiveTemplate = "https://merchantsafeunipay.com/msu/api/v2/post/sale3d/{0}"
    };

    /// <summary>VakıfPayS endpoint yapılandırması.</summary>
    public static readonly ApiV2EndpointConfig VakifPays = new()
    {
        ApiUrlTest = "https://testpos.vakifpays.com.tr/vakifpays/api/v2",
        ApiUrlLive = "https://pos.vakifpays.com.tr/vakifpays/api/v2",
        Sale3DUrlTestTemplate = "https://testpos.vakifpays.com.tr/vakifpays/api/v2/post/sale3d/{0}",
        Sale3DUrlLiveTemplate = "https://pos.vakifpays.com.tr/vakifpays/api/v2/post/sale3d/{0}"
    };

    /// <summary>ZiraatPay endpoint yapılandırması.</summary>
    public static readonly ApiV2EndpointConfig ZiraatPay = new()
    {
        ApiUrlTest = "https://test.ziraatpay.com.tr/ziraatpay/api/v2",
        ApiUrlLive = "https://vpos.ziraatpay.com.tr/ziraatpay/api/v2",
        Sale3DUrlTestTemplate = "https://test.ziraatpay.com.tr/ziraatpay/api/v2/post/sale3d/{0}",
        Sale3DUrlLiveTemplate = "https://vpos.ziraatpay.com.tr/ziraatpay/api/v2/post/sale3d/{0}",
        FraudMetrixOrgId = "6bmm5c3v"
    };
}

using Microsoft.Extensions.DependencyInjection;
using TriPay.Core.Gateways;
using TriPay.Services.Interfaces;
using TriPay.Services.Providers.Ahlpay;
using TriPay.Services.Providers.Akbank;
using TriPay.Services.Providers.AkbankNestpay;
using TriPay.Services.Providers.AlternatifBank;
using TriPay.Services.Providers.Anadolubank;
using TriPay.Services.Providers.Cardplus;
using TriPay.Services.Providers.Denizbank;
using TriPay.Services.Providers.HalkOde;
using TriPay.Services.Providers.IQmoney;
using TriPay.Services.Providers.FinansbankNestpay;
using TriPay.Services.Providers.Garanti;
using TriPay.Services.Providers.Halkbank;
using TriPay.Services.Providers.KuveytTurk;
using TriPay.Services.Providers.ING;
using TriPay.Services.Providers.IsBankasi;
using TriPay.Services.Providers.Iyzico;
using TriPay.Services.Providers.Moka;
using TriPay.Services.Providers.ParamPos;
using TriPay.Services.Providers.Paratika;
using TriPay.Services.Providers.Parolapara;
using TriPay.Services.Providers.PayBull;
using TriPay.Services.Providers.PayNKolay;
using TriPay.Services.Providers.Paynet;
using TriPay.Services.Providers.PaytenMsu;
using TriPay.Services.Providers.QNBFinansbank;
using TriPay.Services.Providers.QNBpay;
using TriPay.Services.Providers.Sekerbank;
using TriPay.Services.Providers.Sipay;
using TriPay.Services.Providers.Tami;
using TriPay.Services.Providers.Vepara;
using TriPay.Services.Providers.TurkEkonomiBankasi;
using TriPay.Services.Providers.TurkiyeFinans;
using TriPay.Services.Providers.Vakifbank;
using TriPay.Services.Providers.VakifKatilim;
using TriPay.Services.Providers.VakifPays;
using TriPay.Services.Providers.YapiKredi;
using TriPay.Services.Providers.Ziraat;
using TriPay.Services.Providers.ZiraatPay;

namespace TriPay.Services.DependencyInjection;

/// <summary>Tüm sanal POS provider'larının DI ve factory eşlemesini merkezi olarak yönetir.</summary>
internal static class PaymentGatewayProviderRegistration
{
    /// <summary>Gateway kodu → provider tipi eşlemesini döndürür.</summary>
    internal static IReadOnlyDictionary<string, Type> ProviderTypes { get; } =
        new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            [PaymentGatewayNames.VakifPays] = typeof(VakifPaysGatewayProvider),
            [PaymentGatewayNames.Iyzico] = typeof(IyzicoGatewayProvider),
            [PaymentGatewayNames.Vakifbank] = typeof(VakifbankGatewayProvider),
            [PaymentGatewayNames.Akbank] = typeof(AkbankGatewayProvider),
            [PaymentGatewayNames.AkbankNestpay] = typeof(AkbankNestpayGatewayProvider),
            [PaymentGatewayNames.AlternatifBank] = typeof(AlternatifBankGatewayProvider),
            [PaymentGatewayNames.Anadolubank] = typeof(AnadolubankGatewayProvider),
            [PaymentGatewayNames.Halkbank] = typeof(HalkbankGatewayProvider),
            [PaymentGatewayNames.ING] = typeof(INGGatewayProvider),
            [PaymentGatewayNames.IsBankasi] = typeof(IsBankasiGatewayProvider),
            [PaymentGatewayNames.FinansbankNestpay] = typeof(FinansbankNestpayGatewayProvider),
            [PaymentGatewayNames.Sekerbank] = typeof(SekerbankGatewayProvider),
            [PaymentGatewayNames.TurkEkonomiBankasi] = typeof(TurkEkonomiBankasiGatewayProvider),
            [PaymentGatewayNames.TurkiyeFinans] = typeof(TurkiyeFinansGatewayProvider),
            [PaymentGatewayNames.Ziraat] = typeof(ZiraatGatewayProvider),
            [PaymentGatewayNames.Cardplus] = typeof(CardplusGatewayProvider),
            [PaymentGatewayNames.Denizbank] = typeof(DenizbankGatewayProvider),
            [PaymentGatewayNames.QNBFinansbank] = typeof(QNBFinansbankGatewayProvider),
            [PaymentGatewayNames.Garanti] = typeof(GarantiGatewayProvider),
            [PaymentGatewayNames.YapiKredi] = typeof(YapiKrediGatewayProvider),
            [PaymentGatewayNames.KuveytTurk] = typeof(KuveytTurkGatewayProvider),
            [PaymentGatewayNames.VakifKatilim] = typeof(VakifKatilimGatewayProvider),
            [PaymentGatewayNames.Sipay] = typeof(SipayGatewayProvider),
            [PaymentGatewayNames.QNBpay] = typeof(QNBpayGatewayProvider),
            [PaymentGatewayNames.PayBull] = typeof(PayBullGatewayProvider),
            [PaymentGatewayNames.Parolapara] = typeof(ParolaparaGatewayProvider),
            [PaymentGatewayNames.IQmoney] = typeof(IQmoneyGatewayProvider),
            [PaymentGatewayNames.Vepara] = typeof(VeparaGatewayProvider),
            [PaymentGatewayNames.HalkOde] = typeof(HalkOdeGatewayProvider),
            [PaymentGatewayNames.ParamPos] = typeof(ParamPosGatewayProvider),
            [PaymentGatewayNames.Paratika] = typeof(ParatikaGatewayProvider),
            [PaymentGatewayNames.PaytenMsu] = typeof(PaytenMsuGatewayProvider),
            [PaymentGatewayNames.Ahlpay] = typeof(AhlpayGatewayProvider),
            [PaymentGatewayNames.Moka] = typeof(MokaGatewayProvider),
            [PaymentGatewayNames.ZiraatPay] = typeof(ZiraatPayGatewayProvider),
            [PaymentGatewayNames.Tami] = typeof(TamiGatewayProvider),
            [PaymentGatewayNames.PayNKolay] = typeof(PayNKolayGatewayProvider),
            [PaymentGatewayNames.Paynet] = typeof(PaynetGatewayProvider),
        };

    /// <summary>Tüm provider tiplerini DI'a scoped olarak kaydeder.</summary>
    internal static IServiceCollection AddPaymentGatewayProviders(this IServiceCollection services)
    {
        foreach (var providerType in ProviderTypes.Values.Distinct())
            services.AddScoped(providerType);

        return services;
    }
}

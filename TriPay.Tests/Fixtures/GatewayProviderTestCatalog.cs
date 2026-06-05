using System.Net;
using TriPay.Core.Gateways;
using TriPay.Services.DependencyInjection;
using TriPay.Services.Providers.CcPayment;
using TriPay.Services.Providers.Iyzico;
using TriPay.Services.Providers.Protocols.ApiV2;
using TriPay.Services.Providers.Nestpay;
using TriPay.Services.Providers.Vakifbank;

namespace TriPay.Tests.Fixtures;

/// <summary>Kayıtlı tüm gateway provider test girdileri.</summary>
public static class GatewayProviderTestCatalog
{
    /// <summary>Test edilecek tüm gateway kayıtları.</summary>
    public static IReadOnlyList<GatewayTestEntry> All { get; } = Build();

    private static IReadOnlyList<GatewayTestEntry> Build()
        => PaymentGatewayProviderRegistration.ProviderTypes
            .Select(kv => new GatewayTestEntry(
                kv.Key,
                kv.Value,
                DetectProtocol(kv.Key, kv.Value),
                ResolveSettings(kv.Key, kv.Value),
                CreateCallbackRequest(kv.Key, kv.Value)))
            .OrderBy(x => x.GatewayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static GatewayProtocolKind DetectProtocol(string gatewayName, Type providerType)
    {
        if (typeof(ApiV2ProtocolGatewayBase).IsAssignableFrom(providerType))
            return GatewayProtocolKind.ApiV2;
        if (typeof(NestpayGatewayBase).IsAssignableFrom(providerType))
            return GatewayProtocolKind.Nestpay;
        if (typeof(CcPaymentGatewayBase).IsAssignableFrom(providerType))
            return GatewayProtocolKind.CcPayment;
        if (providerType == typeof(VakifbankGatewayProvider))
            return GatewayProtocolKind.Vakifbank;
        if (providerType == typeof(IyzicoGatewayProvider))
            return GatewayProtocolKind.Iyzico;

        return gatewayName switch
        {
            PaymentGatewayNames.ParamPos => GatewayProtocolKind.ParamPos,
            PaymentGatewayNames.Moka => GatewayProtocolKind.Moka,
            PaymentGatewayNames.PayNKolay => GatewayProtocolKind.PayNKolay,
            PaymentGatewayNames.Paynet => GatewayProtocolKind.Paynet,
            PaymentGatewayNames.Ahlpay => GatewayProtocolKind.Ahlpay,
            PaymentGatewayNames.Tami => GatewayProtocolKind.Tami,
            _ => GatewayProtocolKind.MdStatusBank
        };
    }

    private static Dictionary<string, string> ResolveSettings(string gatewayName, Type providerType)
    {
        if (typeof(ApiV2ProtocolGatewayBase).IsAssignableFrom(providerType))
            return GatewayTestSettings.ApiV2();
        if (typeof(NestpayGatewayBase).IsAssignableFrom(providerType))
            return GatewayTestSettings.Standard();
        if (typeof(CcPaymentGatewayBase).IsAssignableFrom(providerType))
            return GatewayTestSettings.CcPayment();
        if (providerType == typeof(VakifbankGatewayProvider))
            return GatewayTestSettings.Vakifbank();
        if (providerType == typeof(IyzicoGatewayProvider))
            return GatewayTestSettings.Iyzico();

        return gatewayName switch
        {
            PaymentGatewayNames.Tami => GatewayTestSettings.Tami(),
            PaymentGatewayNames.Ahlpay => GatewayTestSettings.Ahlpay(),
            _ => GatewayTestSettings.Standard()
        };
    }

    private static PaymentGatewayCallbackRequestDto CreateCallbackRequest(string gatewayName, Type providerType)
    {
        return gatewayName switch
        {
            PaymentGatewayNames.Akbank => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["responseCode"] = "VPS-0000",
                    ["mdStatus"] = "1",
                    ["orderId"] = "ORDER-TEST-1"
                }
            },
            PaymentGatewayNames.Denizbank or PaymentGatewayNames.QNBFinansbank => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["ProcReturnCode"] = "00",
                    ["OrderId"] = "ORDER-TEST-1"
                }
            },
            PaymentGatewayNames.VakifKatilim => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["ResponseCode"] = "00",
                    ["MerchantOrderId"] = "ORDER-TEST-1"
                }
            },
            PaymentGatewayNames.KuveytTurk => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["AuthenticationResponse"] = WebUtility.UrlEncode("""
                        <VPosTransactionResponseContract>
                          <ResponseCode>00</ResponseCode>
                          <MerchantOrderId>ORDER-TEST-1</MerchantOrderId>
                        </VPosTransactionResponseContract>
                        """)
                }
            },
            PaymentGatewayNames.YapiKredi => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["mdStatus"] = "1",
                    ["Xid"] = "ORDER-TEST-1"
                }
            },
            _ => CreateCallbackByProtocol(gatewayName, providerType)
        };
    }

    private static PaymentGatewayCallbackRequestDto CreateCallbackByProtocol(string gatewayName, Type providerType)
    {
        var protocol = DetectProtocol(gatewayName, providerType);
        return protocol switch
        {
            GatewayProtocolKind.ApiV2 => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["responseCode"] = "00",
                    ["merchantPaymentId"] = "ORDER-TEST-1",
                    ["pgTranId"] = "TX-TEST-1"
                }
            },
            GatewayProtocolKind.Nestpay => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["mdStatus"] = "1",
                    ["oid"] = "ORDER-TEST-1"
                }
            },
            GatewayProtocolKind.CcPayment => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["md_status"] = "1",
                    ["invoice_id"] = "ORDER-TEST-1"
                }
            },
            GatewayProtocolKind.Vakifbank => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["Status"] = "Y",
                    ["VerifyEnrollmentRequestId"] = "REQ-1",
                    ["SessionInfo"] = "ORDER-TEST-1"
                }
            },
            GatewayProtocolKind.Iyzico => new PaymentGatewayCallbackRequestDto
            {
                IsSuccess = true,
                PaymentId = "pay-test-1",
                ConversationId = "ORDER-TEST-1",
                PaymentStatus = "SUCCESS"
            },
            GatewayProtocolKind.ParamPos => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["mdStatus"] = "1",
                    ["orderId"] = "ORDER-TEST-1"
                }
            },
            GatewayProtocolKind.Moka => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["OtherTrxCode"] = "ORDER-TEST-1",
                    ["trxCode"] = "TX-1"
                }
            },
            GatewayProtocolKind.PayNKolay => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["CLIENT_REFERENCE_CODE"] = "ORDER-TEST-1",
                    ["RESPONSE_CODE"] = "2"
                }
            },
            GatewayProtocolKind.Paynet => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["session_id"] = "SESSION-1",
                    ["token_id"] = "TOKEN-1"
                }
            },
            GatewayProtocolKind.Ahlpay => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["orderId"] = "ORDER-TEST-1"
                }
            },
            GatewayProtocolKind.Tami => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["orderId"] = "ORDER-TEST-1",
                    ["success"] = "true"
                }
            },
            _ => new PaymentGatewayCallbackRequestDto
            {
                RawData = new Dictionary<string, string>
                {
                    ["mdstatus"] = "1",
                    ["mdStatus"] = "1",
                    ["oid"] = "ORDER-TEST-1",
                    ["orderid"] = "ORDER-TEST-1"
                }
            }
        };
    }
}

/// <summary>Tek bir gateway için test meta verisi.</summary>
/// <param name="GatewayName">Kanal kodu.</param>
/// <param name="ProviderType">Provider CLR tipi.</param>
/// <param name="Protocol">Protokol grubu.</param>
/// <param name="Settings">Sahte gateway ayarları.</param>
/// <param name="CallbackRequest">Başarılı callback örneği.</param>
public sealed record GatewayTestEntry(
    string GatewayName,
    Type ProviderType,
    GatewayProtocolKind Protocol,
    IReadOnlyDictionary<string, string> Settings,
    PaymentGatewayCallbackRequestDto CallbackRequest);

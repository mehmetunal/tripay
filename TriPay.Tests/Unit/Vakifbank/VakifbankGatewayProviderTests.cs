using TriPay.Services.Models;
using TriPay.Infrastructure.Gateways;
using TriPay.Services.Providers.Vakifbank;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.Vakifbank;

/// <summary>Vakıfbank gateway provider unit testleri.</summary>
public sealed class VakifbankGatewayProviderTests
{
    [Fact]
    public async Task GetInstallmentInfo_VakifbankBin_TaksitListesiDondurur()
    {
        var provider = CreateProvider();
        var result = await provider.GetInstallmentInfoAsync(new PaymentGatewayInstallmentRequestDto
        {
            CardNumber = "4938410157705590",
            Amount = 1000m
        });

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Data!.Installments, x => x.Count == 3);
    }

    [Fact]
    public async Task GetInstallmentInfo_YabanciBin_TekCekim()
    {
        var provider = CreateProvider(binPrefixes: "411111");
        var result = await provider.GetInstallmentInfoAsync(new PaymentGatewayInstallmentRequestDto
        {
            CardNumber = "4938410157705590",
            Amount = 500m
        });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!.Installments);
        Assert.Equal(1, result.Data.Installments[0].Count);
    }

    [Fact]
    public void NormalizeCallbackFromRawData_StatusY_Success()
    {
        var provider = CreateProvider();
        var n = provider.NormalizeCallbackFromRawData(new Dictionary<string, string>
        {
            ["Status"] = "Y",
            ["VerifyEnrollmentRequestId"] = "req-1",
            ["SessionInfo"] = "order-9"
        });

        Assert.Equal("success", n.Status);
        Assert.Equal("order-9", n.ConversationId);
    }

    [Fact]
    public async Task InitializePayment_EnrollmentBasarili_HtmlDondurur()
    {
        var provider = CreateProvider(http: new FakeHttpClientFactory(new FakeHttpMessageHandler(VakifbankTestXml.EnrollmentSuccess)));
        var result = await provider.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
        {
            Payment = new PaymentRequest
            {
                CardNumber = "4938410157705590",
                ExpiryMonth = "12",
                ExpiryYear = "2030",
                Cvv = "123",
                Amount = 100m,
                Currency = "TRY",
                OrderNumber = "ORD-VB-1",
                ReturnUrl = "https://localhost/callback"
            }
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data!.RedirectHtml);
        Assert.Contains("acs.test", result.Data.RedirectHtml);
        Assert.Equal("ORD-VB-1", result.Data.ConversationId);
    }

    [Fact]
    public async Task InitializePayment_NotEnrolled_OzelMesaj()
    {
        var provider = CreateProvider(http: new FakeHttpClientFactory(new FakeHttpMessageHandler(VakifbankTestXml.EnrollmentNotEnrolled)));
        var result = await provider.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
        {
            Payment = new PaymentRequest
            {
                CardNumber = "4111111111111111",
                ExpiryMonth = "12",
                ExpiryYear = "2030",
                Cvv = "123",
                Amount = 50m,
                OrderNumber = "ORD-FAIL",
                ReturnUrl = "https://localhost/callback"
            }
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("3D Secure", result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessCallback_BasarisizStatus_HataDondurur()
    {
        var provider = CreateProvider();
        var result = await provider.ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto
        {
            RawData = new Dictionary<string, string>
            {
                ["Status"] = "N",
                ["ErrorCode"] = "0051"
            }
        });

        Assert.False(result.IsSuccess);
    }

    private static VakifbankGatewayProvider CreateProvider(
        string binPrefixes = "493841",
        IHttpClientFactory? http = null)
        => new(
            new FakeGatewaySettings(PaymentGatewayNames.Vakifbank, new Dictionary<string, string>
            {
                ["MerchantId"] = "m",
                ["MerchantPassword"] = "p",
                ["TerminalNo"] = "t",
                ["EnrollmentUrl"] = "https://3dsecuretest.vakifbank.com.tr/MPIAPI/MPI_Enrollment.aspx",
                ["VerifyUrl"] = "https://onlineodemetest.vakifbank.com.tr/VposService/v3/Vposreq.aspx",
                ["ResultCodeSuccess"] = "0000",
                ["ThreeDsStatusEnrolled"] = "Y",
                ["ThreeDsStatusAttempt"] = "A",
                ["ThreeDsStatusNotEnrolled"] = "N",
                ["ErrorCodeIssuerException"] = "1001",
                ["InstallmentCounts"] = "3,6",
                ["BinPrefixes"] = binPrefixes
            }),
            http ?? new FakeHttpClientFactory(),
            new TriPay.Infrastructure.Redis.InMemoryVakifbankSaleStateStore(),
            InMemoryGatewayMetadataService.CreateWithVakifbankDefaults(),
            new TestLogger<VakifbankGatewayProvider>());
}

using TriPay.Services.Providers.Iyzico.Helpers;

namespace TriPay.Tests.Unit.Iyzico;

/// <summary>Iyzico fraud/onay kuralları unit testleri.</summary>
public sealed class IyzicoPaymentHelperTests
{
    [Fact]
    public void IsMerchantApproved_SuccessVeSuccess_True()
    {
        Assert.True(IyzicoPaymentHelper.IsMerchantApproved("success", "SUCCESS", IyzicoPaymentHelper.FraudApproved));
    }

    [Fact]
    public void IsMerchantApproved_FailureStatus_False()
    {
        Assert.False(IyzicoPaymentHelper.IsMerchantApproved("failure", "SUCCESS", 1));
    }

    [Fact]
    public void GetFraudUserMessage_UnderReview_TurkceMesaj()
    {
        var msg = IyzicoPaymentHelper.GetFraudUserMessage(IyzicoPaymentHelper.FraudUnderReview);
        Assert.Contains("incelemesine", msg);
    }
}

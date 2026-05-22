using TriPay.Services.Security;

namespace TriPay.Tests.Unit.Security;

/// <summary>PCI maskeleme unit testleri.</summary>
public sealed class PciDataMaskerTests
{
    [Fact]
    public void MaskPan_SonDortHaneGorunur()
    {
        var masked = PciDataMasker.MaskPan("4938410157705590");
        Assert.EndsWith("5590", masked);
        Assert.DoesNotContain("493841015770", masked);
    }

    [Fact]
    public void MaskSensitivePayload_CvvMaskeler()
    {
        var json = """{"cardNumber":"4938410157705590","cvv":"123"}""";
        var masked = PciDataMasker.MaskSensitivePayload(json);
        Assert.DoesNotContain("4938410157705590", masked);
        Assert.Contains("***", masked);
    }
}

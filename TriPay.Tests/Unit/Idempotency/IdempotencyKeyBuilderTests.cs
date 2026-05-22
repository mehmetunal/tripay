using TriPay.Core.Idempotency;

namespace TriPay.Tests.Unit.Idempotency;

/// <summary>Idempotency anahtar üretimi unit testleri.</summary>
public sealed class IdempotencyKeyBuilderTests
{
    [Fact]
    public void ForCallback_AyniGirdi_AyniAnahtar()
    {
        var a = IdempotencyKeyBuilder.ForCallback("Vakifbank", "pay-1", "success");
        var b = IdempotencyKeyBuilder.ForCallback("Vakifbank", "pay-1", "success");
        Assert.Equal(a, b);
    }

    [Fact]
    public void ForCallback_FarkliStatus_FarkliAnahtar()
    {
        var success = IdempotencyKeyBuilder.ForCallback("Vakifbank", "pay-1", "success");
        var failure = IdempotencyKeyBuilder.ForCallback("Vakifbank", "pay-1", "failure");
        Assert.NotEqual(success, failure);
    }

    [Fact]
    public void ForAuth3DS_GatewayVePaymentIdIcerir()
    {
        var key = IdempotencyKeyBuilder.ForAuth3DS("Iyzico", "PAY-9");
        Assert.Contains("Iyzico", key);
        Assert.Contains("PAY-9", key);
    }
}

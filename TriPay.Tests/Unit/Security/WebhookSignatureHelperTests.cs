using TriPay.Services.Security;

namespace TriPay.Tests.Unit.Security;

/// <summary>Webhook HMAC imza unit testleri.</summary>
public sealed class WebhookSignatureHelperTests
{
    [Fact]
    public void Validate_GecerliImza_KabulEder()
    {
        const string secret = "test-secret";
        const string payload = """{"order":"1"}""";
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var sig = WebhookSignatureHelper.ComputeSignature(payload, ts, secret);
        Assert.True(WebhookSignatureHelper.Validate(payload, sig, ts, secret));
    }

    [Fact]
    public void Validate_YanlisSecret_Redder()
    {
        const string payload = "{}";
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var sig = WebhookSignatureHelper.ComputeSignature(payload, ts, "a");
        Assert.False(WebhookSignatureHelper.Validate(payload, sig, ts, "b"));
    }

    [Fact]
    public void Validate_EskiTimestamp_Redder()
    {
        const string secret = "s";
        const string payload = "{}";
        var ts = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds().ToString();
        var sig = WebhookSignatureHelper.ComputeSignature(payload, ts, secret);
        Assert.False(WebhookSignatureHelper.Validate(payload, sig, ts, secret));
    }
}

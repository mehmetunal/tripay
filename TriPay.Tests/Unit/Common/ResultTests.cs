using TriPay.Core.Common;

namespace TriPay.Tests.Unit.Common;

/// <summary><see cref="Result{T}"/> sarmalayıcı davranış testleri.</summary>
public sealed class ResultTests
{
    [Fact]
    public void Success_DataVeIsSuccessDogrudur()
    {
        var result = Result<string>.Success("ok");
        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Data);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Failure_HataMesajiVeDataAyrimi()
    {
        var result = Result<int>.Failure("hata", 42);
        Assert.False(result.IsSuccess);
        Assert.Equal("hata", result.ErrorMessage);
        Assert.Equal(42, result.Data);
    }
}

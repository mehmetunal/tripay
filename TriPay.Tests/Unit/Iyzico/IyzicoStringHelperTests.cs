using TriPay.Services.Providers.Iyzico.Helpers;

namespace TriPay.Tests.Unit.Iyzico;

/// <summary>Iyzico string yardımcı unit testleri.</summary>
public sealed class IyzicoStringHelperTests
{
    [Fact]
    public void SplitFullName_IkiKelime_Ayrilir()
    {
        var (name, surname) = IyzicoStringHelper.SplitFullName("Mehmet Unal");
        Assert.Equal("Mehmet", name);
        Assert.Equal("Unal", surname);
    }

    [Fact]
    public void SplitFullName_BosVarsayilanDeger()
    {
        var (name, surname) = IyzicoStringHelper.SplitFullName(null);
        Assert.Equal("Musteri", name);
        Assert.Equal("Adi", surname);
    }
}

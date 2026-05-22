using System.Xml;
using TriPay.Services.Providers.Vakifbank;
using TriPay.Services.Providers.Vakifbank.Helpers;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.Vakifbank;

/// <summary>Vakıfbank XML parse unit testleri.</summary>
public sealed class VakifbankXmlHelperTests
{
    [Fact]
    public void ParseVposResponse_BasariliXml_IsSuccessTrue()
    {
        var doc = new XmlDocument();
        doc.LoadXml(VakifbankTestXml.VposSuccess);
        var vpos = VakifbankXmlHelper.ParseVposResponse(doc);

        Assert.True(vpos.IsSuccessWithCode(VakifbankConstants.ResultCodeSuccess));
        Assert.Equal(VakifbankConstants.ResultCodeSuccess, vpos.ResultCode);
        Assert.Equal("VPOS-TX-99", vpos.TransactionId);
    }
}

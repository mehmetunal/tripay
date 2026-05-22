namespace TriPay.Tests.Fixtures;

/// <summary>Vakıfbank MPI/VPOS test XML örnekleri.</summary>
public static class VakifbankTestXml
{
    /// <summary>3D enrollment başarılı (Y) yanıtı.</summary>
    public const string EnrollmentSuccess = """
        <?xml version="1.0" encoding="utf-8"?>
        <IPaySecure>
          <Message>
            <VERes>
              <Status>Y</Status>
              <PaReq>PA-REQ-TEST</PaReq>
              <TermUrl>https://bank.test/term</TermUrl>
              <MD>MD-TEST</MD>
              <ACSUrl>https://acs.test/3d</ACSUrl>
            </VERes>
          </Message>
        </IPaySecure>
        """;

    /// <summary>3D kayıtsız kart (N) + issuer exception.</summary>
    public const string EnrollmentNotEnrolled = """
        <?xml version="1.0" encoding="utf-8"?>
        <IPaySecure>
          <ErrorMessage>Issuer Exception</ErrorMessage>
          <MessageErrorCode>1001</MessageErrorCode>
          <Message>
            <VERes>
              <Status>N</Status>
            </VERes>
          </Message>
        </IPaySecure>
        """;

    /// <summary>VPOS satış başarılı.</summary>
    public const string VposSuccess = """
        <?xml version="1.0" encoding="utf-8"?>
        <VposResponse>
          <ResultCode>0000</ResultCode>
          <ResultDetail>Islem basarili</ResultDetail>
          <TransactionId>VPOS-TX-99</TransactionId>
        </VposResponse>
        """;
}

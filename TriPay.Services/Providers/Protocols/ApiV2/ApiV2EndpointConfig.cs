namespace TriPay.Services.Providers.Protocols.ApiV2;

/// <summary>API v2 (SESSIONTOKEN + form POST) protokolü endpoint yapılandırması.</summary>
public sealed class ApiV2EndpointConfig
{
    /// <summary>Test ortamı API kök adresi.</summary>
    public required string ApiUrlTest { get; init; }

    /// <summary>Canlı ortam API kök adresi.</summary>
    public required string ApiUrlLive { get; init; }

    /// <summary>Test ortamı 3D sale endpoint şablonu ({0} = session token).</summary>
    public required string Sale3DUrlTestTemplate { get; init; }

    /// <summary>Canlı ortam 3D sale endpoint şablonu ({0} = session token).</summary>
    public required string Sale3DUrlLiveTemplate { get; init; }

    /// <summary>Online Metrix org_id; dolu ise 3D HTML'e fraud script eklenir.</summary>
    public string? FraudMetrixOrgId { get; init; }
}

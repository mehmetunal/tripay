namespace TriPay.Admin.Application.Dtos.Gateways;

public sealed class GatewayListDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public sealed class GatewaySettingListDto
{
    public int Id { get; init; }
    public int PaymentGatewayId { get; init; }
    public string SettingKey { get; init; } = string.Empty;
    public string SettingValue { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public sealed class GatewaySettingEditDto
{
    public int Id { get; init; }
    public int PaymentGatewayId { get; init; }
    public string GatewayCode { get; init; } = string.Empty;
    public string SettingKey { get; init; } = string.Empty;
    public string SettingValue { get; init; } = string.Empty;
    public string Environment { get; init; } = "All";
    public bool IsActive { get; init; } = true;
}

public sealed class UpsertGatewaySettingDto
{
    public int Id { get; init; }
    public int PaymentGatewayId { get; init; }
    public string GatewayCode { get; init; } = string.Empty;
    public required string SettingKey { get; init; }
    public required string SettingValue { get; init; }
    public required string Environment { get; init; }
    public bool IsActive { get; init; }
}

public sealed class GatewayErrorListDto
{
    public int Id { get; init; }
    public int PaymentGatewayId { get; init; }
    public string ProviderErrorCode { get; init; } = string.Empty;
    public string? NormalizedCode { get; init; }
    public string UserMessage { get; init; } = string.Empty;
    public string Locale { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public sealed class GatewayErrorEditDto
{
    public int Id { get; init; }
    public int PaymentGatewayId { get; init; }
    public string GatewayCode { get; init; } = string.Empty;
    public string ProviderErrorCode { get; init; } = string.Empty;
    public string? NormalizedCode { get; init; }
    public string UserMessage { get; init; } = string.Empty;
    public string Locale { get; init; } = "tr";
    public bool IsActive { get; init; } = true;
}

public sealed class UpsertGatewayErrorDto
{
    public int Id { get; init; }
    public int PaymentGatewayId { get; init; }
    public string GatewayCode { get; init; } = string.Empty;
    public required string ProviderErrorCode { get; init; }
    public string? NormalizedCode { get; init; }
    public required string UserMessage { get; init; }
    public required string Locale { get; init; }
    public bool IsActive { get; init; }
}

public sealed class GatewayContextDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

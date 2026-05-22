namespace TriPay.Admin.Application.Dtos.Merchants;

public sealed class MerchantListDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ApiKeyMasked { get; init; } = string.Empty;
    public string? WebhookUrl { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class MerchantEditDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? WebhookUrl { get; init; }
    public bool IsActive { get; init; }
    public string ApiKeyMasked { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class UpdateMerchantDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? WebhookUrl { get; init; }
    public bool IsActive { get; init; }
}

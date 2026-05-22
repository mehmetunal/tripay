using TriPay.Admin.Application.Dtos;
using TriPay.Admin.Application.Dtos.Dashboard;
using TriPay.Admin.Application.Dtos.Gateways;
using TriPay.Admin.Application.Dtos.Merchants;
using TriPay.Admin.Application.Dtos.Outbox;
using TriPay.Admin.Application.Dtos.System;
using TriPay.Admin.Application.Dtos.Transactions;
using TriPay.Admin.Models.Outbox;
using TriPay.Admin.Models.Dashboard;
using TriPay.Admin.Models.Gateways;
using TriPay.Admin.Models.Merchants;
using TriPay.Admin.Models.Shared;
using TriPay.Admin.Models.System;
using TriPay.Admin.Models.Transactions;
using TriPay.Data.Entities;
using TriPay.Data.Repositories.Admin;

namespace TriPay.Admin.Application.Mappings;

/// <summary>ViewModel ↔ DTO dönüşümleri (controller ince kalır).</summary>
public static class AdminDtoMapper
{
    public static PagedResultDto<TDest> ToPagedDto<TSource, TDest>(
        AdminPagedResult<TSource> source,
        Func<TSource, TDest> map) =>
        new()
        {
            Items = source.Items.Select(map).ToList(),
            Page = source.Page,
            PageSize = source.PageSize,
            TotalCount = source.TotalCount
        };

    public static PagedResult<T> ToPagedViewModel<T>(PagedResultDto<T> dto) =>
        new()
        {
            Items = dto.Items.ToList(),
            Page = dto.Page,
            PageSize = dto.PageSize,
            TotalCount = dto.TotalCount
        };

    public static TransactionListQueryDto ToQueryDto(TransactionListFilter filter) => new()
    {
        OrderNumber = filter.OrderNumber,
        Status = filter.Status,
        MerchantId = filter.MerchantId,
        PaymentGatewayId = filter.PaymentGatewayId,
        FromUtc = filter.FromUtc,
        ToUtc = filter.ToUtc,
        Page = filter.Page,
        PageSize = filter.PageSize
    };

    public static AdminTransactionQuery ToRepositoryQuery(TransactionListQueryDto dto) => new()
    {
        OrderNumber = dto.OrderNumber,
        Status = dto.Status,
        MerchantId = dto.MerchantId,
        PaymentGatewayId = dto.PaymentGatewayId,
        FromUtc = dto.FromUtc,
        ToUtc = dto.ToUtc,
        Page = dto.Page,
        PageSize = dto.PageSize
    };

    public static TransactionListItem ToListItem(TransactionListDto dto) => new()
    {
        Id = dto.Id,
        OrderNumber = dto.OrderNumber,
        MerchantName = dto.MerchantName,
        GatewayCode = dto.GatewayCode,
        Amount = dto.Amount,
        Currency = dto.Currency,
        Status = dto.Status,
        CreatedAt = dto.CreatedAt
    };

    public static TransactionDetailViewModel ToDetailViewModel(TransactionDetailDto dto) => new()
    {
        Id = dto.Id,
        OrderNumber = dto.OrderNumber,
        MerchantName = dto.MerchantName,
        GatewayCode = dto.GatewayCode,
        Amount = dto.Amount,
        Currency = dto.Currency,
        Status = dto.Status,
        ExternalTransactionId = dto.ExternalTransactionId,
        ResponseCode = dto.ResponseCode,
        ResponseMessage = dto.ResponseMessage,
        ClientIp = dto.ClientIp,
        InstallmentCount = dto.InstallmentCount,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt ?? dto.CreatedAt,
        Logs = dto.Logs.Select(l => new TransactionLogItem
        {
            Id = l.Id,
            LogType = l.LogType,
            Direction = l.Direction,
            GatewayCode = l.GatewayCode,
            HttpStatusCode = l.HttpStatusCode,
            ErrorCode = l.ErrorCode,
            RequestPayload = l.RequestPayload,
            ResponsePayload = l.ResponsePayload,
            CreatedAt = l.CreatedAt
        }).ToList()
    };

    public static MerchantListItem ToListItem(MerchantListDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        ApiKeyMasked = dto.ApiKeyMasked,
        WebhookUrl = dto.WebhookUrl,
        IsActive = dto.IsActive,
        CreatedAt = dto.CreatedAt
    };

    public static MerchantEditViewModel ToEditViewModel(MerchantEditDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        WebhookUrl = dto.WebhookUrl,
        IsActive = dto.IsActive,
        ApiKeyMasked = dto.ApiKeyMasked,
        CreatedAt = dto.CreatedAt
    };

    public static UpdateMerchantDto ToUpdateDto(MerchantEditViewModel model) => new()
    {
        Id = model.Id,
        Name = model.Name.Trim(),
        WebhookUrl = string.IsNullOrWhiteSpace(model.WebhookUrl) ? null : model.WebhookUrl.Trim(),
        IsActive = model.IsActive
    };

    public static OutboxListItem ToListItem(OutboxListDto dto) => new()
    {
        Id = dto.Id,
        TransactionId = dto.TransactionId,
        RoutingKey = dto.RoutingKey,
        IsPublished = dto.IsPublished,
        RetryCount = dto.RetryCount,
        CreatedAt = dto.CreatedAt,
        PublishedAt = dto.PublishedAt,
        PayloadPreview = dto.PayloadPreview
    };

    public static GatewaySettingEditViewModel ToSettingViewModel(GatewaySettingEditDto dto) => new()
    {
        Id = dto.Id,
        PaymentGatewayId = dto.PaymentGatewayId,
        GatewayCode = dto.GatewayCode,
        SettingKey = dto.SettingKey,
        SettingValue = dto.SettingValue,
        Environment = dto.Environment,
        IsActive = dto.IsActive
    };

    public static UpsertGatewaySettingDto ToUpsertDto(GatewaySettingEditViewModel model) => new()
    {
        Id = model.Id,
        PaymentGatewayId = model.PaymentGatewayId,
        GatewayCode = model.GatewayCode,
        SettingKey = model.SettingKey.Trim(),
        SettingValue = model.SettingValue.Trim(),
        Environment = model.Environment,
        IsActive = model.IsActive
    };

    public static GatewayErrorEditViewModel ToErrorViewModel(GatewayErrorEditDto dto) => new()
    {
        Id = dto.Id,
        PaymentGatewayId = dto.PaymentGatewayId,
        GatewayCode = dto.GatewayCode,
        ProviderErrorCode = dto.ProviderErrorCode,
        NormalizedCode = dto.NormalizedCode,
        UserMessage = dto.UserMessage,
        Locale = dto.Locale,
        IsActive = dto.IsActive
    };

    public static UpsertGatewayErrorDto ToUpsertDto(GatewayErrorEditViewModel model) => new()
    {
        Id = model.Id,
        PaymentGatewayId = model.PaymentGatewayId,
        GatewayCode = model.GatewayCode,
        ProviderErrorCode = model.ProviderErrorCode.Trim(),
        NormalizedCode = string.IsNullOrWhiteSpace(model.NormalizedCode) ? null : model.NormalizedCode.Trim(),
        UserMessage = model.UserMessage.Trim(),
        Locale = model.Locale,
        IsActive = model.IsActive
    };

    public static DashboardViewModel ToViewModel(DashboardStatsDto dto) => new()
    {
        TransactionCount = dto.TransactionCount,
        SuccessCount = dto.SuccessCount,
        FailedCount = dto.FailedCount,
        PendingOutboxCount = dto.PendingOutboxCount,
        MerchantCount = dto.MerchantCount,
        GatewayCount = dto.GatewayCount,
        DatabaseOk = dto.DatabaseOk,
        RedisOk = dto.RedisOk
    };

    public static SystemStatusViewModel ToViewModel(SystemStatusDto dto) => new()
    {
        DatabaseOk = dto.DatabaseOk,
        RedisOk = dto.RedisOk,
        LatestMigrationVersion = dto.LatestMigrationVersion,
        LatestMigrationDescription = dto.LatestMigrationDescription,
        UseInMemoryDatabase = dto.UseInMemoryDatabase,
        RabbitMqEnabled = dto.RabbitMqEnabled,
        AllowedIpRanges = dto.AllowedIpRanges.ToList()
    };

    public static GatewayListDto ToGatewayListDto(PaymentGatewayRecord g) => new()
    {
        Id = g.Id,
        Code = g.Code,
        DisplayName = g.DisplayName,
        IsActive = g.IsActive
    };

    public static GatewayContextDto ToGatewayContext(PaymentGatewayRecord g) => new()
    {
        Id = g.Id,
        Code = g.Code,
        DisplayName = g.DisplayName
    };

    public static GatewaySettingListDto ToSettingListDto(GatewaySetting s) => new()
    {
        Id = s.Id,
        PaymentGatewayId = s.PaymentGatewayId,
        SettingKey = s.SettingKey,
        SettingValue = s.SettingValue,
        Environment = s.Environment,
        IsActive = s.IsActive
    };

    public static GatewayErrorListDto ToErrorListDto(GatewayErrorMapping e) => new()
    {
        Id = e.Id,
        PaymentGatewayId = e.PaymentGatewayId,
        ProviderErrorCode = e.ProviderErrorCode,
        NormalizedCode = e.NormalizedCode,
        UserMessage = e.UserMessage,
        Locale = e.Locale,
        IsActive = e.IsActive
    };
}

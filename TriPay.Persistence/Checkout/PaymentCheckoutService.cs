using System.Diagnostics;
using System.Text.Json;
using TriPay.Core.Common;
using TriPay.Core.Redis;
using TriPay.Data.Constants;
using TriPay.Data.Entities;
using TriPay.Data.Repositories;
using TriPay.Services;
using TriPay.Services.Checkout;
using TriPay.Services.Interfaces;
using TriPay.Services.Messaging;
using TriPay.Services.Models;
using TriPay.Services.Providers.VakifPays.Models;
using TriPay.Core.Options;
using TriPay.Services.Security;
using Microsoft.Extensions.Options;

namespace TriPay.Persistence.Checkout;

/// <summary><see cref="IPaymentCheckoutService"/> — DB işlem, log ve gateway orchestration.</summary>
public sealed class PaymentCheckoutService : IPaymentCheckoutService
{
    private readonly IPaymentTransactionRepository _transactions;
    private readonly IPaymentGatewayService _gateway;
    private readonly IRedisDistributedLock _distributedLock;
    private readonly IRedisRateLimiter _rateLimiter;
    private readonly TriPayPersistenceOptions _persistence;

    /// <summary>Checkout servisi oluşturur.</summary>
    public PaymentCheckoutService(
        IPaymentTransactionRepository transactions,
        IPaymentGatewayService gateway,
        IRedisDistributedLock distributedLock,
        IRedisRateLimiter rateLimiter,
        IOptions<TriPayPersistenceOptions> persistence)
    {
        _transactions = transactions;
        _gateway = gateway;
        _distributedLock = distributedLock;
        _rateLimiter = rateLimiter;
        _persistence = persistence.Value;
    }

    /// <summary>Ödemeyi başlatır; işlem ve log kayıtlarını oluşturur.</summary>
    public async Task<Result<PaymentGatewayInitializeResponseDto>> PayAsync(
        PaymentRequest model,
        string gatewayName,
        CancellationToken cancellationToken = default)
    {
        var merchantId = await _transactions.GetDefaultMerchantIdAsync(cancellationToken);
        if (!await _rateLimiter.AllowAsync(merchantId, cancellationToken))
            return Result<PaymentGatewayInitializeResponseDto>.Failure("İstek limiti aşıldı. Lütfen kısa süre sonra tekrar deneyin.");

        var gatewayId = await _transactions.GetGatewayIdByCodeAsync(gatewayName, cancellationToken)
            ?? await _transactions.GetGatewayIdByCodeAsync(PaymentGatewayNames.Default, cancellationToken);

        if (gatewayId == null)
            return Result<PaymentGatewayInitializeResponseDto>.Failure("Ödeme kanalı veritabanında bulunamadı.");

        var existing = await _transactions.GetByOrderAsync(merchantId, model.OrderNumber, cancellationToken);
        if (existing != null && existing.Status == TransactionStatuses.Success)
            return Result<PaymentGatewayInitializeResponseDto>.Failure("Bu sipariş numarası ile başarılı ödeme zaten var.");

        var now = DateTime.UtcNow;
        PaymentTransaction entity;

        if (existing != null)
        {
            entity = existing;
            entity.Amount = model.Amount;
            entity.Currency = model.Currency ?? "TRY";
            entity.InstallmentCount = model.InstallmentCount;
            entity.ClientIp = model.CustomerIp;
            entity.Status = TransactionStatuses.Pending;
            entity.UpdatedAt = now;
            await _transactions.UpdateAsync(entity, cancellationToken);
        }
        else
        {
            entity = new PaymentTransaction
            {
                MerchantId = merchantId,
                PaymentGatewayId = gatewayId.Value,
                OrderNumber = model.OrderNumber,
                Amount = model.Amount,
                Currency = model.Currency ?? "TRY",
                InstallmentCount = model.InstallmentCount,
                ClientIp = model.CustomerIp,
                Status = TransactionStatuses.Pending,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _transactions.AddAsync(entity, cancellationToken);
        }

        await _transactions.AddLogAsync(new PaymentTransactionLog
        {
            TransactionId = entity.Id,
            LogType = TransactionLogTypes.PayRequest,
            Direction = LogDirections.Inbound,
            RequestPayload = PciDataMasker.MaskSensitivePayload(JsonSerializer.Serialize(model)),
            GatewayCode = gatewayName,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        var sw = Stopwatch.StartNew();
        var initRequest = new PaymentGatewayInitializeRequestDto
        {
            GatewayName = gatewayName,
            Payment = model
        };

        await _transactions.AddLogAsync(new PaymentTransactionLog
        {
            TransactionId = entity.Id,
            LogType = TransactionLogTypes.InitializeRequest,
            Direction = LogDirections.Outbound,
            RequestPayload = PciDataMasker.MaskSensitivePayload(JsonSerializer.Serialize(initRequest)),
            GatewayCode = gatewayName,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        var result = await _gateway.InitializePaymentAsync(initRequest, gatewayName);
        sw.Stop();

        var responsePayload = result.IsSuccess
            ? PciDataMasker.MaskSensitivePayload(JsonSerializer.Serialize(result.Data))
            : result.ErrorMessage ?? string.Empty;

        await _transactions.AddLogAsync(new PaymentTransactionLog
        {
            TransactionId = entity.Id,
            LogType = TransactionLogTypes.InitializeResponse,
            Direction = LogDirections.Inbound,
            ResponsePayload = responsePayload,
            GatewayCode = gatewayName,
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage,
            DurationMs = (int)sw.ElapsedMilliseconds,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        if (result.IsSuccess && result.Data != null)
        {
            entity.ExternalTransactionId = result.Data.PaymentId ?? result.Data.ConversationId;
            entity.UpdatedAt = DateTime.UtcNow;
            if (!result.Data.Success)
            {
                entity.Status = TransactionStatuses.Failed;
                entity.ResponseMessage = result.Data.Message;
            }
            await _transactions.UpdateAsync(entity, cancellationToken);
        }

        return result;
    }

    /// <summary>3D callback işler; tutar doğrulaması veritabanından yapılır.</summary>
    public async Task<CheckoutCallbackResult> ProcessCallbackAsync(
        Dictionary<string, string> rawData,
        string gatewayName,
        CancellationToken cancellationToken = default)
    {
        var merchantId = await _transactions.GetDefaultMerchantIdAsync(cancellationToken);

        var callbackResult = await _gateway.ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto
        {
            GatewayName = gatewayName,
            RawData = rawData
        }, gatewayName);

        var callback = callbackResult.Data ?? new PaymentGatewayCallbackResponseDto
        {
            ErrorMessage = callbackResult.ErrorMessage ?? string.Empty
        };

        var orderNumber = callback.OrderNumber;
        if (string.IsNullOrWhiteSpace(orderNumber))
            orderNumber = rawData.GetValueOrDefault("orderId")
                ?? rawData.GetValueOrDefault("OrderId")
                ?? rawData.GetValueOrDefault("VerifyEnrollmentRequestId")
                ?? string.Empty;

        var transaction = !string.IsNullOrWhiteSpace(orderNumber)
            ? await _transactions.GetByOrderAsync(merchantId, orderNumber, cancellationToken)
            : null;

        if (transaction == null)
        {
            return new CheckoutCallbackResult
            {
                Success = false,
                Message = "İşlem kaydı bulunamadı.",
                OrderNumber = orderNumber,
                CallbackFieldsDisplay = string.Join(Environment.NewLine, rawData.Select(x => $"{x.Key}: {x.Value}"))
            };
        }

        var lockHandle = await _distributedLock.TryAcquireAsync(
            RedisKeyNames.TransactionLock(transaction.Id), cancellationToken);
        if (lockHandle == null)
        {
            return new CheckoutCallbackResult
            {
                Success = false,
                Message = "İşlem başka bir istek tarafından işleniyor.",
                OrderNumber = orderNumber
            };
        }

        await using (lockHandle)
        {
            await _transactions.AddLogAsync(new PaymentTransactionLog
            {
                TransactionId = transaction.Id,
                LogType = TransactionLogTypes.CallbackRequest,
                Direction = LogDirections.Inbound,
                RequestPayload = PciDataMasker.MaskSensitivePayload(JsonSerializer.Serialize(rawData)),
                GatewayCode = gatewayName,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            var sw = Stopwatch.StartNew();
            var queryResult = await _gateway.GetPaymentStatusAsync(
                string.IsNullOrWhiteSpace(callback.OrderNumber) ? orderNumber : callback.OrderNumber,
                gatewayName);
            sw.Stop();

            var query = queryResult.Data ?? new PaymentGatewayStatusResponseDto { ResponseCode = string.Empty };

            await _transactions.AddLogAsync(new PaymentTransactionLog
            {
                TransactionId = transaction.Id,
                LogType = TransactionLogTypes.CallbackResponse,
                Direction = LogDirections.Outbound,
                ResponsePayload = PciDataMasker.MaskSensitivePayload(JsonSerializer.Serialize(callback)),
                GatewayCode = gatewayName,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await _transactions.AddLogAsync(new PaymentTransactionLog
            {
                TransactionId = transaction.Id,
                LogType = TransactionLogTypes.QueryRequest,
                Direction = LogDirections.Outbound,
                RequestPayload = JsonSerializer.Serialize(new { orderNumber, gatewayName }),
                GatewayCode = gatewayName,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await _transactions.AddLogAsync(new PaymentTransactionLog
            {
                TransactionId = transaction.Id,
                LogType = TransactionLogTypes.QueryResponse,
                Direction = LogDirections.Inbound,
                ResponsePayload = PciDataMasker.MaskSensitivePayload(JsonSerializer.Serialize(query)),
                GatewayCode = gatewayName,
                DurationMs = (int)sw.ElapsedMilliseconds,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            var amountText = rawData.GetValueOrDefault("amount") ?? string.Empty;
            var callbackAmountOk = true;
            if (decimal.TryParse(amountText.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var cbAmount))
            {
                callbackAmountOk = Math.Abs(cbAmount - transaction.Amount) < 0.01m;
            }

            var success = callbackAmountOk && callback.Success && query.Success && query.ResponseCode == "00";

            transaction.Status = success ? TransactionStatuses.Success : TransactionStatuses.Failed;
            transaction.ResponseCode = callback.ResponseCode ?? query.ResponseCode;
            transaction.ResponseMessage = success ? "Ödeme doğrulandı." : callback.Message;
            transaction.ExternalTransactionId = callback.TransactionId ?? transaction.ExternalTransactionId;
            transaction.UpdatedAt = DateTime.UtcNow;
            await _transactions.UpdateAsync(transaction, cancellationToken);

            if (success)
                await EnqueueWebhookAsync(transaction, cancellationToken);

            var message = success
                ? "Ödeme doğrulandı."
                : !callbackAmountOk
                    ? "Callback tutarı veritabanı kaydı ile uyuşmuyor."
                    : $"Callback/Query doğrulaması başarısız. CallbackCode={callback.ResponseCode}, QueryCode={query.ResponseCode}";

            return new CheckoutCallbackResult
            {
                Success = success,
                Message = message,
                OrderNumber = orderNumber,
                TransactionId = callback.TransactionId,
                ResponseCode = callback.ResponseCode ?? string.Empty,
                CallbackMessage = callback.Message ?? string.Empty,
                ErrorMessage = callback.ErrorMessage ?? string.Empty,
                QueryResponseCode = query.ResponseCode ?? string.Empty,
                CallbackFieldsDisplay = string.Join(Environment.NewLine, rawData.Select(x => $"{x.Key}: {x.Value}")),
                AmountText = amountText
            };
        }
    }

    /// <summary>Taksit bilgisini sorgular (demo: DB log atlanır — işlem FK zorunlu).</summary>
    public async Task<Result<InstallmentInfoResponse>> GetInstallmentsAsync(
        string cardNumber,
        decimal amount,
        string gatewayName,
        CancellationToken cancellationToken = default)
    {
        var result = await _gateway.GetInstallmentInfoAsync(new PaymentGatewayInstallmentRequestDto
        {
            CardNumber = cardNumber,
            Amount = amount,
            GatewayName = gatewayName,
            TestPlatform = true
        }, gatewayName);

        if (!result.IsSuccess)
            return Result<InstallmentInfoResponse>.Failure(result.ErrorMessage ?? "Taksit sorgusu başarısız.");

        return Result<InstallmentInfoResponse>.Success(result.Data ?? new InstallmentInfoResponse());
    }

    private async Task EnqueueWebhookAsync(PaymentTransaction transaction, CancellationToken cancellationToken)
    {
        if (!_persistence.EnableOutbox)
            return;

        var message = new PaymentWebhookMessage
        {
            TransactionId = transaction.Id,
            MerchantId = transaction.MerchantId,
            OrderNumber = transaction.OrderNumber,
            Status = transaction.Status,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            OccurredAtUtc = DateTime.UtcNow
        };

        await _transactions.AddOutboxAsync(new OutboxMessage
        {
            TransactionId = transaction.Id,
            Payload = JsonSerializer.Serialize(message),
            RoutingKey = "payment.webhook",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }
}

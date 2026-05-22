using TriPay.Services.PaymentGateways.Providers;

namespace TriPay.Services.PaymentGateways.Models;

public class PaymentInitializeRequest
{
    public PaymentRequest Payment { get; set; } = new();
    public string? GatewayName { get; set; }
}

public class PaymentGatewayInitializeRequestDto : PaymentInitializeRequest
{
}

public class PaymentGatewayInitializeResponseDto
{
    public bool Success { get; set; }
    public bool IsSuccess { get => Success; set => Success = value; }
    public string Message { get; set; } = string.Empty;
    public string? RedirectHtml { get; set; }
    public string? RedirectUrl { get; set; }
}

public class PaymentInitializeResponse : PaymentGatewayInitializeResponseDto
{
}

public class PaymentCallbackRequest
{
    public Dictionary<string, string> RawData { get; set; } = new();
}

public class PaymentGatewayCallbackRequestDto : PaymentCallbackRequest
{
    public string? GatewayName { get; set; }
}

public class PaymentGatewayCallbackResponseDto
{
    public bool Success { get; set; }
    public bool IsSuccess { get => Success; set => Success = value; }
    public string Message { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string? ConversationId { get => OrderNumber; set => OrderNumber = value ?? string.Empty; }
    public string TransactionId { get; set; } = string.Empty;
    public string? PaymentId { get => TransactionId; set => TransactionId = value ?? string.Empty; }
    public string? PaymentStatus { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? Currency { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class PaymentCallbackResponse : PaymentGatewayCallbackResponseDto
{
}

public class InstallmentInfoRequest
{
    public string CardNumber { get; set; } = string.Empty;
    public string? BinNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get => Amount; set => Amount = value; }
    public string Currency { get; set; } = "TRY";
    public string? GatewayName { get; set; }
    public bool TestPlatform { get; set; } = true;
}

public class InstallmentOptionDto
{
    public int Count { get; set; }
    public decimal Rate { get; set; }
    public decimal Monthly { get; set; }
    public decimal Total { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class InstallmentInfoResponse
{
    public bool Success { get; set; }
    public bool IsSuccess { get => Success; set => Success = value; }
    public List<InstallmentOptionDto> Installments { get; set; } = new();
}

public class PaymentGatewayInstallmentRequestDto : InstallmentInfoRequest
{
}

public class PaymentGatewayInstallmentResponseDto : InstallmentInfoResponse
{
}

public class PaymentGatewayStatusResponseDto
{
    public bool Success { get; set; }
    public bool IsSuccess { get => Success; set => Success = value; }
    public string Message { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public Dictionary<string, object>? Raw { get; set; }
}

public class PaymentStatusResponse : PaymentGatewayStatusResponseDto
{
}

public class PaymentGatewayAuth3DSRequestDto : PaymentCallbackRequest
{
}

public class PaymentGatewayAuth3DSResponseDto : PaymentGatewayCallbackResponseDto
{
}

public class PaymentGatewayRefundResponseDto : CancelRefundResponse
{
}

namespace TriPay.Services.Diagnostics;

/// <summary>Ödeme tanılama kayıtlarını alan hedef (UI deposu, dosya vb.).</summary>
public interface IPaymentDiagnosticSink
{
    /// <summary>Yeni tanılama kaydını yazar.</summary>
    void Write(PaymentDiagnosticEntry entry);
}

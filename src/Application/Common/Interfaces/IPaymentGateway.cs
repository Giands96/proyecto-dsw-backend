namespace Application.Common.Interfaces;

public record PaymentResult(bool Success, string? ProviderReference = null, string? Error = null);

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(decimal amount, CancellationToken cancellationToken = default);
}

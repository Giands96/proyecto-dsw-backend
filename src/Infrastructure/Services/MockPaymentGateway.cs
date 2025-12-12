using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(decimal amount, CancellationToken cancellationToken = default)
    {
        // Simple mock: always approve
        return Task.FromResult(new PaymentResult(true, ProviderReference: Guid.NewGuid().ToString()));
    }
}

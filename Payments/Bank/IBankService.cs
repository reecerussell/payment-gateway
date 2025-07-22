namespace Payments.Bank;

public interface IBankService
{
    Task<AuthorizePaymentResponse> AuthorizePaymentAsync(AuthorizePaymentRequest request, CancellationToken cancellationToken);

    Task<bool> HealthAsync(CancellationToken cancellationToken);
}
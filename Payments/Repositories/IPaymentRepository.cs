using Payments.Models;

namespace Payments.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetAsync(string id, CancellationToken cancellationToken);

    Task CreateAsync(string id, CancellationToken cancellationToken);
}
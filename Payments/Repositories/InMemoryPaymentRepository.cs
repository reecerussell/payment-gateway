using Payments.Models;

namespace Payments.Repositories;

public class InMemoryPaymentRepository : IPaymentRepository
{
    public Task<Payment?> GetAsync(string id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task CreateAsync(string id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
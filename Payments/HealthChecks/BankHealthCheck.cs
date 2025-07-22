using Microsoft.Extensions.Diagnostics.HealthChecks;
using Payments.Bank;

namespace Payments.HealthChecks;

public class BankHealthCheck : IHealthCheck
{
    private readonly IBankService _bank;

    public BankHealthCheck(IBankService bank)
    {
        _bank = bank;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        var healthy = await _bank.HealthAsync(cancellationToken);

        return healthy ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded();
    }
}
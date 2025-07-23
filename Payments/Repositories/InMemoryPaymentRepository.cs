using System.Collections.Concurrent;
using System.Diagnostics;
using Payments.Abstractions.Exceptions;
using Payments.Models;

namespace Payments.Repositories;

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly ILogger<InMemoryPaymentRepository> _logger;
    private readonly ConcurrentDictionary<string, Payment> _payments;

    public InMemoryPaymentRepository(ILogger<InMemoryPaymentRepository> logger)
    {
        _logger = logger;
        _payments = new ConcurrentDictionary<string, Payment>();
    }
    
    public Task<Payment?> GetAsync(string id, CancellationToken cancellationToken)
    {
        using var activity = Telemetry.ActivitySource.StartActivity();
        activity?.SetTag("payment_id", id);
        
        _logger.LogDebug("Attempting to retrieve payment '{paymentId}' from dictionary", id);

        if (_payments.TryGetValue(id, out var payment))
        {
            _logger.LogDebug("Successfully retrieved payment from dictionary");
        }
        else
        {
            _logger.LogDebug("Payment '{paymentId}' could not be found in dictionary", id);
        }
        
        return Task.FromResult(payment);
    }

    public Task CreateAsync(Payment payment, CancellationToken cancellationToken)
    {
        using var activity = Telemetry.ActivitySource.StartActivity();
        activity?.SetTag("payment_id", payment.Id);
        
        _logger.LogDebug("Attempting to add payment '{paymentId}' into dictionary", payment.Id);
        
        if (!_payments.TryAdd(payment.Id.ToString(), payment))
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            
            throw new InternalServerErrorException("Failed to add payment record into concurrent dictionary");
        }
        
        _logger.LogDebug("Successfully added payment '{paymentId}' into dictionary", payment.Id);
        
        return Task.CompletedTask;
    }
}
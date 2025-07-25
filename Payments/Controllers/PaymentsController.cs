using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;
using Payments.Abstractions;
using Payments.Abstractions.Exceptions;
using Payments.Bank;
using Payments.Models;
using Payments.Repositories;

namespace Payments.Controllers;

[ApiController]
[Route("payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _repository;
    private readonly IBankService _bank;
    private readonly ISystemClock _systemClock;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentRepository repository,
        IBankService bank,
        ISystemClock systemClock,
        ILogger<PaymentsController> logger)
    {
        _repository = repository;
        _bank = bank;
        _systemClock = systemClock;
        _logger = logger;
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType<PaymentResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ApiError>((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType<ApiError>((int)HttpStatusCode.NotFound)]
    public async Task<PaymentResponse> GetAsync(string id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request to retieve payment '{paymentId}'", id);

        var payment = await _repository.GetAsync(id, cancellationToken);
        if (payment == null)
        {
            _logger.LogInformation("Payment '{paymentId}' could not be found, returning {exceptionType} error",
                id, ExceptionTypes.PaymentNotFound);

            throw new PaymentNotFoundException();
        }
        
        _logger.LogInformation("Successfully found payment '{paymentId}', sending response", id);
        
        return MapPaymentToResponse(payment);
    }

    [HttpPost]
    [ProducesResponseType<PaymentResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ApiError>((int)HttpStatusCode.BadRequest)]
    public async Task<PaymentResponse> PostAsync([FromBody] PaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = Payment.Create(request, _systemClock.UtcNow.UtcDateTime);
        
        _logger.LogInformation("Received request to process a payment for {amount} ({currency}) for card ending {lastFourCardDigits}",
            payment.Amount, payment.Currency, payment.LastFourCardDigits);

        var authResponse = await _bank.AuthorizePaymentAsync(new AuthorizePaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth}/{request.ExpiryYear}",
            Amount = request.Amount,
            Currency = request.Currency,
            Cvv = request.Cvv
        }, cancellationToken);

        if (authResponse.Authorized)
        {
            _logger.LogInformation("Received payment authorization with code '{authCode}****'", authResponse.AuthorizationCode[..4]);
            
            payment.MarkAsAuthorized(authResponse.AuthorizationCode);
        }
        else
        {
            _logger.LogInformation("Payment was not authorized, marking it as declined.");
        }

        try
        {
            _logger.LogInformation("Saving record for payment '{paymentId}'", payment.Id);

            await _repository.CreateAsync(payment, cancellationToken);

            _logger.LogInformation("Successfully saved payment record '{paymentId}'", payment.Id);
        }
        catch (Exception)
        {
            // If we have processed the payment but failed to save the payment record, we raise
            // the event below which we can look out for in an observability and alerting solution.
            Activity.Current?.AddEvent(new ActivityEvent("OrphanedPayment"));
            
            throw;
        }

        return MapPaymentToResponse(payment);
    }

    private static PaymentResponse MapPaymentToResponse(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id.ToString(),
            Amount = payment.Amount,
            Currency = payment.Currency,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            LastFourCardDigits = payment.LastFourCardDigits,
            Status = payment.Status
        };
    }
}
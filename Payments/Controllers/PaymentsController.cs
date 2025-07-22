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
        throw new InternalServerErrorException("Not implemented");
    }

    [HttpPost]
    [ProducesResponseType<PaymentResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ApiError>((int)HttpStatusCode.BadRequest)]
    public async Task<PaymentResponse> PostAsync([FromBody] PaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = Payment.Create(request, _systemClock.UtcNow.UtcDateTime);

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
            payment.MarkAsAuthorized(authResponse.AuthorizationCode);
        }

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
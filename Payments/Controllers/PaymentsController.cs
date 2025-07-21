using System.Net;
using Microsoft.AspNetCore.Mvc;
using Payments.Bank;
using Payments.Dtos;
using Payments.Exceptions;
using Payments.Repositories;

namespace Payments.Controllers;

[ApiController]
[Route("payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _repository;
    private readonly IBankService _bank;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentRepository repository, IBankService bank, ILogger<PaymentsController> logger)
    {
        _repository = repository;
        _bank = bank;
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
        throw new InternalServerErrorException("Not implemented");
    }
}
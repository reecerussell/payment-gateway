using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Payments.Bank;

public class HttpBankService : IBankService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<HttpBankService> _logger;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public HttpBankService(IHttpClientFactory clientFactory, ILogger<HttpBankService> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }
    
    public async Task<AuthorizePaymentResponse> AuthorizePaymentAsync(AuthorizePaymentRequest request, CancellationToken cancellationToken)
    {
        using var activity = Telemetry.ActivitySource.StartActivity();
        
        _logger.LogInformation("Sending authorize payment request for card ending '{lastFourCardDigits}'", request.CardNumber[^4..]);
        
        using var client = _clientFactory.CreateClient(Constants.HttpClientName);
        
        var payload = JsonSerializer.Serialize(request, _jsonSerializerOptions);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/payments", content, cancellationToken);
        var data = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                _logger.LogInformation("Successfully received response for authorize payment request");
                activity?.SetStatus(ActivityStatusCode.Ok);
                return (await JsonSerializer.DeserializeAsync<AuthorizePaymentResponse>(data, _jsonSerializerOptions, cancellationToken))!;
            case HttpStatusCode.BadRequest:
                _logger.LogError("Received BadRequest response when sending authorize payment request");
                var error = (await JsonSerializer.DeserializeAsync<ErrorResponse>(data, _jsonSerializerOptions, cancellationToken))!;
                activity?.SetStatus(ActivityStatusCode.Error, "Bad Request");
                throw new BankException(error.ErrorMessage);
            case HttpStatusCode.ServiceUnavailable:
                _logger.LogError("Received service unavailable response from Bank when sending authorize payment request");
                activity?.SetStatus(ActivityStatusCode.Error, "Service Unavailable");
                throw new BankException("Bank is currently unavailable");
            default:
                _logger.LogError("Received and unexpected status code '{statusCode}' when sending authorize payment request", response.StatusCode);
                activity?.SetStatus(ActivityStatusCode.Error, "Unexpected Status Code");
                throw new BankException($"Received an unsupported status code {response.StatusCode}");
        }
    }

    public async Task<bool> HealthAsync(CancellationToken cancellationToken)
    {
        using var activity = Telemetry.ActivitySource.StartActivity();
        
        _logger.LogInformation("Sending health check request to Bank");
        
        using var client = _clientFactory.CreateClient(Constants.HttpClientName);
        var response = await client.GetAsync("/", cancellationToken);
        
        _logger.LogInformation("Received health check response with status code '{statusCode}'", response.StatusCode);

        activity?.SetStatus(response.IsSuccessStatusCode ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
        
        return response.IsSuccessStatusCode;
    }
}
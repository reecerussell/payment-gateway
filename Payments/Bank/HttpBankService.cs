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
        using var client = _clientFactory.CreateClient(Constants.HttpClientName);
        
        var payload = JsonSerializer.Serialize(request, _jsonSerializerOptions);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/payments", content, cancellationToken);
        var data = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return (await JsonSerializer.DeserializeAsync<AuthorizePaymentResponse>(data, _jsonSerializerOptions, cancellationToken))!;
            case HttpStatusCode.BadRequest:
                var error = (await JsonSerializer.DeserializeAsync<ErrorResponse>(data, _jsonSerializerOptions, cancellationToken))!;
                throw new BankException(error.ErrorMessage);
            case HttpStatusCode.ServiceUnavailable:
                throw new BankException("Bank is currently unavailable");
            default:
                throw new BankException($"Received an unsupported status code {response.StatusCode}");
        }
    }

    public async Task<bool> HealthAsync(CancellationToken cancellationToken)
    {
        using var client = _clientFactory.CreateClient(Constants.HttpClientName);
        var response = await client.GetAsync("/", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
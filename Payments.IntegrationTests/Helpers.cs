using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Payments.Abstractions;

namespace Payments.IntegrationTests;

public static class Helpers
{
    private const string ApiBaseUrl = "http://localhost:8080";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
    
    public static async Task<ApiResult<PaymentResponse>> PostPaymentAsync(PaymentRequest request)
    {
        using var client = new HttpClient();
        var content = new StringContent(JsonSerializer.Serialize(request, JsonSerializerOptions));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync(ApiBaseUrl + "/payments", content);
        var json = await response.Content.ReadAsStringAsync();
        
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = JsonSerializer.Deserialize<ApiError>(json, JsonSerializerOptions)!;

            return new ApiResult<PaymentResponse> { Error = error! };
        }

        var data = JsonSerializer.Deserialize<PaymentResponse>(json, JsonSerializerOptions)!;

        return new ApiResult<PaymentResponse> { Data = data };
    }
}

public record ApiResult<T>
{
    public T? Data { get; init;  }
    public ApiError? Error { get; init; }
}
using Microsoft.Extensions.Http.Resilience;

namespace Payments.Bank;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBankService(this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["BANK_ADDRESS"];
        if (string.IsNullOrEmpty(address))
        {
            throw new ArgumentException("The bank address has not been configured");
        }

        if (!Uri.TryCreate(address, UriKind.Absolute, out var addressUri))
        {
            throw new ArgumentException($"The bank address '{address}' is not a URI");
        }

        services.AddHttpClient(Constants.HttpClientName, options =>
        {
            options.BaseAddress = addressUri;
            options.Timeout = TimeSpan.FromSeconds(30);
        }).AddStandardResilienceHandler(options =>
        {
            options.Retry = new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 2, // Total 3 requests
            };
        });

        return services.AddSingleton<IBankService, HttpBankService>();
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Internal;
using Payments.Bank;
using Payments.HealthChecks;
using Payments.Middleware;
using Payments.Repositories;
using Serilog;
using Swashbuckle.AspNetCore.ReDoc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Payments;

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();

        // Boilerplate services
        builder.Services.AddLogging(o =>
            o.ClearProviders().AddSerilog(dispose: false));
        builder.Services.AddTelemetry(builder.Configuration);
        builder.Services.AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(ConfigureSwagger);
        builder.Services.AddHealthChecks()
            .AddCheck<BankHealthCheck>("Bank", HealthStatus.Degraded);

        // App services
        builder.Services
            .AddBankService(builder.Configuration)
            .AddSingleton<ISystemClock, SystemClock>()
            .AddSingleton<ErrorMiddleware>()
            .AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();

        var app = builder.Build();
        
        app.UseSwagger(o => o.RouteTemplate = "docs/{documentName}/openapi.json");
        app.UseReDoc(ConfigureReDoc);
        app.UseMiddleware<ErrorMiddleware>();
        app.MapHealthChecks("/health");
        app.MapControllers();

        app.Run();
    }
    
    private static void ConfigureSwagger(SwaggerGenOptions options)
    {
        options.SupportNonNullableReferenceTypes();
    }
    
    private static void ConfigureReDoc(ReDocOptions options)
    {
        options.RoutePrefix = "docs";
        options.DocumentTitle = "Payments Gateway";
    }
}
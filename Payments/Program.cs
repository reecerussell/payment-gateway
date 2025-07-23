using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Internal;
using OpenTelemetry.Trace;
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
    private static readonly ManualResetEvent Shutdown = new (false);
    private static readonly ManualResetEvent Done = new(false);
    
    public static async Task Main(string[] args)
    {
        ConfigureLogger();
        ConfigureShutdown();
        
        // Configure and start the HTTP server
        var app = WebApplication.CreateBuilder(args)
            .ConfigureServices()
            .ConfigureApp();
        await app.StartAsync();

        await WaitForShutdownAsync(app);
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

    private static void ConfigureLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
    }

    private static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
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

        return builder;
    }

    private static WebApplication ConfigureApp(this WebApplicationBuilder builder)
    {
        var app = builder.Build();
        
        app.UseSwagger(o => o.RouteTemplate = "docs/{documentName}/openapi.json");
        app.UseReDoc(ConfigureReDoc);
        app.UseMiddleware<ErrorMiddleware>();
        app.MapHealthChecks("/health");
        app.MapControllers();

        return app;
    }

    /// <summary>
    /// Used to configure shutdown events, such as unhandled exceptions or CTRL+C.
    /// </summary>
    private static void ConfigureShutdown()
    {
        Console.CancelKeyPress += (_, _) =>
        {
            Log.Information("Shutdown signal received!");
            Shutdown.Set();
            Done.WaitOne();
        };

        AppDomain.CurrentDomain.UnhandledException += (_, evt) =>
        {
            if (evt.ExceptionObject is Exception e)
            {
                Log.Error(e, "An unhandled exception occurred");
            }
            else
            {
                Log.Error("An unhandled error occurred but did not contain an exception");
            }

            Shutdown.Set();
            Done.WaitOne();
        };
    }

    /// <summary>
    /// Used to wait for shutdown events and handle clean up.
    /// </summary>
    private static async Task WaitForShutdownAsync(WebApplication app)
    {
        // Wait for shutdown
        Shutdown.WaitOne();
        
        Log.Information("Shutting down Payments Gateway...");

        using var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await app.WaitForShutdownAsync(ctx.Token);
        
        Log.Debug("Flushing OpenTelemetry metrics...");
        
        TracerProvider.Default.ForceFlush(2000);
        TracerProvider.Default.Shutdown(1000);
        
        Log.Information("Shutdown complete");
        
        await Log.CloseAndFlushAsync();

        Done.Set();
    }
}
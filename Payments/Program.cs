using Payments.Bank;
using Payments.Repositories;
using Serilog;
using Swashbuckle.AspNetCore.ReDoc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Payments;

public class Program
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
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.ClearProviders().AddSerilog(dispose: false));
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(ConfigureSwagger);

        // App services
        builder.Services
            .AddHttpClient()
            .AddSingleton<IBankService, HttpBankService>()
            .AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();

        var app = builder.Build();
        
        app.UseSwagger(o => o.RouteTemplate = "docs/{documentName}/openapi.json");
        app.UseReDoc(ConfigureReDoc);
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
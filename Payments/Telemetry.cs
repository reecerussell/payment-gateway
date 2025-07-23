using System.Diagnostics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Payments;

public static class Telemetry
{
    public const string ResourceName = "PaymentsGateway";
    public const string PaymentsSourceName = "Payments";

    public static readonly ActivitySource ActivitySource = new (PaymentsSourceName);

    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var exporterUrl = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(exporterUrl) && !Uri.TryCreate(exporterUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"The OTEL Exporter endpoint '{exporterUrl}' is not a valid URI");
        }

        var environment = configuration["ENVIRONMENT"] ?? "devel";
        
        services.AddOpenTelemetry()
            .ConfigureResource(r =>
                r.AddService(Telemetry.ResourceName)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["service.environment"] = environment
                    }))
            .WithTracing(o =>
            {
                o.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(PaymentsSourceName);

                if (!string.IsNullOrWhiteSpace(exporterUrl))
                {
                    o.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(exporterUrl);
                        
                        // For ease of this solution, we're only using OTLP and GRPC at the moment.
                        otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                    });
                }
            });

        return services;
    }
}
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Thetis.Web.Extensions;

public static class TelemetryExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";
    private const string AspireTelemetryEndpointPath = "http://localhost:4317";
    
    public static TBuilder AddTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "Thetis";
        var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
        var aspireEndpoint =
            Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL")
            ?? builder.Configuration["OpenTelemetry:AspireEndpoint"]
            ?? AspireTelemetryEndpointPath;
        
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation(tracing =>
                        tracing.Filter = context => 
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    .AddHttpClientInstrumentation()
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(!string.IsNullOrEmpty(otlpEndpoint) ? otlpEndpoint : aspireEndpoint);
                    });
            })
            .WithMetrics(metricsBuilder =>
            {
                metricsBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(!string.IsNullOrEmpty(otlpEndpoint) ? otlpEndpoint : aspireEndpoint);
                    });
            });

        
            // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
            //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
            //{
            //    builder.Services.AddOpenTelemetry()
            //       .UseAzureMonitor();
            //}
            
        return builder;
    }
}
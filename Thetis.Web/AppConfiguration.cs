using Microsoft.Extensions.FileProviders;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Thetis.Web;

public static class AppConfiguration
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";
    private const string AspireTelemetryEndpointPath = "http://localhost:4317";
    
    public static void AddSerilog(this WebApplicationBuilder builder)
    {
        var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
        var aspireEndpoint =
            Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL")
            ?? builder.Configuration["OpenTelemetry:AspireEndpoint"]
            ?? AspireTelemetryEndpointPath;
           
        builder.Host.UseSerilog(
            (context, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                    .WriteTo.OpenTelemetry(options =>
                    {
                        options.Endpoint = !string.IsNullOrEmpty(otlpEndpoint) ? otlpEndpoint : aspireEndpoint;
                    })
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId();
            });
    }
    
    public static void AddTelemetry(this WebApplicationBuilder builder)
    {
        var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
        var aspireEndpoint =
            Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL")
            ?? builder.Configuration["OpenTelemetry:AspireEndpoint"]
            ?? AspireTelemetryEndpointPath;
        
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
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
    }
    
    public static void UseBrowserStaticFiles(this IApplicationBuilder app, string contentRootPath)
    {
        var browserPath = Path.Combine(contentRootPath, "wwwroot", "browser");

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = new PhysicalFileProvider(browserPath),
            RequestPath = ""
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(browserPath),
            RequestPath = ""
        });
    }
}
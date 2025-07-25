using FastEndpoints;
using Scalar.AspNetCore;
using Thetis.Profiles.Infrastructure;
using Thetis.Users.Infrastructure;
using Thetis.Web;
using Thetis.Web.Infrastructure;
using Thetis.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();

builder.AddTelemetry();

builder.AddSecurity();

// Add Module Services
builder.Services.AddUserServices(builder.Configuration);
builder.Services.AddProfileServices(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("ExposeOpenApi"))
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Thetis API")
            .WithTheme(ScalarTheme.Default);
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();

app.UseAuthentication();

// Serve static files from the wwwroot/browser directory
app.UseBrowserStaticFiles(builder.Environment.ContentRootPath);

app.UseFastEndpoints(config =>
{
    config.Endpoints.RoutePrefix = "api";
});

app.Run();
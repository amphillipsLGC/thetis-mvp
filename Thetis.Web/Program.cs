using FastEndpoints;
using Microsoft.Extensions.FileProviders;
using Thetis.Profiles.Infrastructure;
using Thetis.Users.Infrastructure;
using Thetis.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Module Services
builder.Services.AddUserServices(builder.Configuration);
builder.Services.AddProfileServices(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "browser")),
    RequestPath = ""
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "browser")),
    RequestPath = ""
});

app.UseFastEndpoints();

//app.MapFallbackToFile("index.html");

app.Run();
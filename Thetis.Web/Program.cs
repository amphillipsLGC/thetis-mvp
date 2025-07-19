using FastEndpoints;
using Thetis.Profiles.Infrastructure;
using Thetis.Users.Infrastructure;

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

app.UseHttpsRedirection();

app.UseFastEndpoints();

app.Run();
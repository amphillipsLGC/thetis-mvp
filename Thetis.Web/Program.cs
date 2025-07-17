using FastEndpoints;
using Thetis.Users;

var builder = WebApplication.CreateBuilder(args);

// Add Module Services
builder.Services.AddUserServices();

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
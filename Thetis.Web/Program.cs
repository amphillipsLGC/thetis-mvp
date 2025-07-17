using Thetis.Users;

var builder = WebApplication.CreateBuilder(args);

// Add Module Services
builder.Services.AddUserServices();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map module endpoints
app.MapUserEndpoints();

app.Run();
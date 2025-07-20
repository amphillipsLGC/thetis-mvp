using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Thetis.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    
    public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current is not null ? Activity.Current.TraceId.ToString() : context.TraceIdentifier;
            var problem = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = $"See traceId: {traceId} for more information.",
                Instance = context.Request.Path,
                Extensions =
                {
                    ["traceId"] = traceId
                }
            };

            if (_env.IsDevelopment())
            {
                problem.Extensions["stackTrace"] = ex.ToString();
            }

            context.Response.StatusCode = problem.Status.Value;
            context.Response.ContentType = "application/problem+json";
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
        }
    }
}
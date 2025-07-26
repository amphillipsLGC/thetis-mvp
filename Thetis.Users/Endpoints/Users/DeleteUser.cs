using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Authorization;
using Thetis.Common.Exceptions;
using Thetis.Users.Application.Services;

namespace Thetis.Users.Endpoints.Users;

internal class DeleteUser(IUserService userService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/users/{id}");
        Description(x => x
            .WithName("Delete a user")
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404)
            .ProducesProblem(500));
        Policies(nameof(PolicyNames.SystemAdministrator));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var id = Route<string>("id");
        
        if(string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var userId))
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid user ID format.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        var result = await userService.DeleteUserAsync(userId, cancellationToken);
        
        await result.Match(
            success => SendAsync(null, StatusCodes.Status204NoContent, cancellation: cancellationToken),
            error => error switch
            {
                EntityNotFoundException => SendAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"User with ID {userId} not found.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                }, StatusCodes.Status404NotFound, cancellation: cancellationToken),
                _ => SendAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = $"An unexpected error occurred while deleting the user. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                }, StatusCodes.Status500InternalServerError, cancellation: cancellationToken)
            }
        );
        

        
    }
}
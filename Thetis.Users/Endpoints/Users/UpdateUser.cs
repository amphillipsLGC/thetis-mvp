using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Authorization;
using Thetis.Common.Exceptions;
using Thetis.Users.Application.Models;
using Thetis.Users.Application.Services;
using Thetis.Users.Domain;

namespace Thetis.Users.Endpoints.Users;

internal class UpdateUser(IUserService userService) : Endpoint<UserModel>
{
    public override void Configure()
    {
        Put("/users/{userId}");
        Description(x => x
            .WithName("Update an existing user")
            .Produces<UserModel>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(500));
        Policies(nameof(PolicyNames.SystemAdministrator));
    }

    public override async Task HandleAsync(UserModel request, CancellationToken cancellationToken)
    {
        var userId = Route<string>("userId");
        
        // Validate the user ID from the route
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var id))
        {           
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid user ID format.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        // Ensure the request body ID matches the route ID
        if (request.Id != id)
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "User ID in the request body does not match the ID in the route.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        var result = await userService.UpdateUserAsync(request, cancellationToken);

        await result.Match(
            success => SendOkAsync(success.ToModel(), cancellation: cancellationToken),
            error => error switch
            {
                EntityNotFoundException => SendAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"User with ID {id} not found.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                }, StatusCodes.Status404NotFound, cancellation: cancellationToken),
                UsernameAlreadyInUseException => SendAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Detail = "The username is already in use. Please choose a different username.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier
                }, StatusCodes.Status409Conflict, cancellation: cancellationToken),
                EmailAlreadyInUseException => SendAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Detail = "The email address is already in use. Please choose a different email.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier
                }, StatusCodes.Status409Conflict, cancellation: cancellationToken),
                _ => SendAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = $"An unexpected error occurred while updating the user. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                }, StatusCodes.Status400BadRequest, cancellation: cancellationToken)
            }
        );
    }
}
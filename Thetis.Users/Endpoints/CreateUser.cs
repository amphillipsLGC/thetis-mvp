using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Users.Application.Models;
using Thetis.Users.Application.Services;

namespace Thetis.Users.Endpoints;

internal class CreateUser(IUserService userService) : Endpoint<UserModel>
{
    public override void Configure()
    {
        Post("/users");
        Description(x => x
            .WithName("Create a new user")
            .Produces<UserModel>(201)
            .ProducesProblem(400)
            .ProducesProblem(500));
        AllowAnonymous();
    }

    public override async Task HandleAsync(UserModel request, CancellationToken cancellationToken)
    {
        var result = await userService.AddUserAsync(request.ToEntity(), cancellationToken);

        await result.Match(
            success => SendAsync(success.ToModel(), StatusCodes.Status201Created, cancellation: cancellationToken),
            error => SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = $"An unexpected error occurred while creating the user. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellation: cancellationToken)
        );
    }
}
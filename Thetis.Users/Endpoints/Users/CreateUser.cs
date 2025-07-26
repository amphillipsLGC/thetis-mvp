using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Authorization;
using Thetis.Users.Application.Models;
using Thetis.Users.Application.Services;
using Thetis.Users.Domain;

namespace Thetis.Users.Endpoints.Users;

internal class CreateUser(IUserService userService) : Endpoint<CreateUserModel>
{
    public override void Configure()
    {
        Post("/users");
        Description(x => x
            .WithName("Create a new user")
            .Produces<UserModel>(201)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(409)
            .ProducesProblem(500));
        Policies(nameof(PolicyNames.SystemAdministrator));
    }

    public override async Task HandleAsync(CreateUserModel request, CancellationToken cancellationToken)
    {
        var result = await userService.CreateUserAsync(request, cancellationToken);

        await result.Match(
            success => 
                SendAsync(success.ToModel(), StatusCodes.Status201Created, cancellation: cancellationToken),
            error => error switch
            {
                UsernameAlreadyInUseException => SendAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Detail = "The username is already in use. Please choose a different username.",
                        TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier
                    }, statusCode: StatusCodes.Status409Conflict, cancellation: cancellationToken
                ),
                EmailAlreadyInUseException => SendAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Detail = "The email address is already in use. Please choose a different email.",
                        TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier
                    }, statusCode: StatusCodes.Status409Conflict, cancellation: cancellationToken
                ),
                _ => SendAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Detail =
                            $"An unexpected error occurred while creating the user. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                        TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                    }, StatusCodes.Status400BadRequest, cancellation: cancellationToken
                )
            }
        );
    }
}
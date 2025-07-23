using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Users.Application.Models;
using Thetis.Users.Application.Services;
using Thetis.Users.Domain;

namespace Thetis.Users.Endpoints.Roles;

internal class CreateRole(IRoleService roleService) : Endpoint<RoleModel>
{
    public override void Configure()
    {
        Post("/roles");
        Description(x => x
            .WithName("Create a new role")
            .Produces<RoleModel>(201)
            .ProducesProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500));
        AllowAnonymous();
    }

    public override async Task HandleAsync(RoleModel request, CancellationToken cancellationToken)
    {
        var result = await roleService.AddRoleAsync(request, cancellationToken);

        await result.Match(
            success => SendAsync(success.ToModel(), StatusCodes.Status201Created, cancellation: cancellationToken),
            error => error switch
            {
                RoleNameAlreadyExistsException => SendAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Detail = "The role name is already in use. Please choose a different name.",
                        TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier
                    }, statusCode: StatusCodes.Status409Conflict, cancellation: cancellationToken
                ),
                _ => SendAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Detail =
                            $"An unexpected error occurred while creating the role. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                        TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                    }, StatusCodes.Status400BadRequest, cancellation: cancellationToken
                )
            }
        );
    }
    
}
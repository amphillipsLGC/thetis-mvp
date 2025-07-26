using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Common.Exceptions;
using Thetis.Users.Application.Models;
using Thetis.Users.Application.Services;
using Thetis.Users.Domain;

namespace Thetis.Users.Endpoints.Roles;

internal class UpdateRole(IRoleService roleService) : Endpoint<RoleModel>
{
    public override void Configure()
    {
        Put("/roles/{roleId}");
        Description(x => x
            .WithName("Update an existing role")
            .Produces<RoleModel>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(500));
        AllowAnonymous();
    }

    public override async Task HandleAsync(RoleModel request, CancellationToken cancellationToken)
    {
        var roleId = Route<string>("roleId");
        
        // Validate the profile ID from the route
        if (string.IsNullOrWhiteSpace(roleId) || !Guid.TryParse(roleId, out var id))
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid role ID format.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        // Ensure the request body ID matches the route ID
        if(request.Id != id)
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Role ID in the request body does not match the ID in the route.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        var result = await roleService.UpdateRoleAsync(request, cancellationToken);

        await result.Match(
            success => SendAsync(success.ToModel(), StatusCodes.Status200OK, cancellation: cancellationToken),
            error => error switch
            {
                EntityNotFoundException => SendAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Detail = "The role to be updated could not be found.",
                        TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier
                    }, statusCode: StatusCodes.Status404NotFound, cancellation: cancellationToken
                ),
                RoleNameAlreadyExistsException => SendAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Detail = "A role with the same name already exists. Please choose a different name.",
                        TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier
                    }, statusCode: StatusCodes.Status409Conflict, cancellation: cancellationToken
                ),
                _ => SendAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Detail =
                            $"An unexpected error occurred while updating the role. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                        TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                    }, StatusCodes.Status400BadRequest, cancellation: cancellationToken
                )
            }
        );
    }
    
}
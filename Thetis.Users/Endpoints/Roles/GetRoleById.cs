using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Authorization;
using Thetis.Common.Exceptions;
using Thetis.Users.Application.Models;
using Thetis.Users.Application.Services;

namespace Thetis.Users.Endpoints.Roles;

internal class GetRoleById(IRoleService roleService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/roles/{id}");
        Description(x => x
            .WithName("GetRoleById")
            .Produces<RoleModel>(200)
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
        
        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var roleId))
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid role ID format.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }

        var result = await roleService.GetRoleByIdAsync(roleId, cancellationToken);

        await result.Match(
            success => SendAsync(success.ToModel(), StatusCodes.Status200OK, cancellation: cancellationToken),
            error => error switch
            {
                EntityNotFoundException => SendAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Role with ID {roleId} not found.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                }, StatusCodes.Status404NotFound, cancellation: cancellationToken),
                _ => SendAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = $"An unexpected error occurred while retrieving the role. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                }, StatusCodes.Status500InternalServerError, cancellation: cancellationToken)
            }
        );
    }
}
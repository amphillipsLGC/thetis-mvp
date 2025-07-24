using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Thetis.Common.Exceptions;
using Thetis.Users.Application.Services;

namespace Thetis.Users.Endpoints.Roles;

internal class DeleteRole(IRoleService roleService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/roles/{id}");
        AllowAnonymous();
        Description(x => x
            .WithName("DeleteRole")
            .Produces<NoContent>(204)
            .ProducesProblem(404)
            .ProducesProblem(500));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var id = Route<string>("id");
        
        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var roleId))
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid profile ID format.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }

        var result = await roleService.DeleteRoleAsync(roleId, cancellationToken);

        await result.Match(
            success => SendAsync(null, StatusCodes.Status204NoContent, cancellation: cancellationToken),
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
                    Detail = $"An unexpected error occurred while deleting the role. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                }, StatusCodes.Status500InternalServerError, cancellation: cancellationToken)
            }
        );

    }
    
}
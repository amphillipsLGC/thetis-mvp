using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Users.Application.Models;
using Thetis.Users.Application.Services;

namespace Thetis.Users.Endpoints.Roles;

internal class ListRolesResponse
{
    public List<RoleModel> Roles { get; set; } = [];
}

internal class ListRoles(IRoleService roleService) : EndpointWithoutRequest<ListRolesResponse>
{
    public override void Configure()
    {
        Get("/roles");
        Description(x => x
            .WithName("List all roles")
            .Produces<ListRolesResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(500));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        // Extract query parameters
        var sortBy = Query<string?>("sortBy", false) ?? "CreatedOn";
        var pageNumber = Query<int?>("pageNumber", false) ?? 1;
        var pageSize = Query<int?>("pageSize", false) ?? 10;
        
        var roles = await roleService.GetRolesAsync(sortBy, pageNumber, pageSize, cancellationToken);
        
        var response = new ListRolesResponse
        {
            Roles = roles.Select(r => r.ToModel()).ToList()
        };
        
        await SendOkAsync(response, cancellation: cancellationToken);
    }
}
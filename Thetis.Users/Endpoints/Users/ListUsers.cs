using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Users.Application.Models;
using Thetis.Users.Application.Services;

namespace Thetis.Users.Endpoints.Users;

internal class ListUsersResponse
{
    public List<UserModel> Users { get; set; } = [];
}

internal class ListUsers(IUserService userService) : EndpointWithoutRequest<ListUsersResponse>
{
    public override void Configure()
    {
        Get("/users");
        Description(x => x
            .WithName("List all users")
            .Produces<ListUsersResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        // Extract query parameters
        var sortBy = Query<string?>("sortBy", false) ?? "CreatedOn";
        var pageNumber = Query<int?>("pageNumber", false) ?? 1;
        var pageSize = Query<int?>("pageSize", false) ?? 10;
        
        var users = await userService.GetUsersAsync(sortBy, pageNumber, pageSize, cancellationToken);
        
        await SendOkAsync(new ListUsersResponse
        {
            Users = users.Select(p => p.ToModel()).ToList()
        }, cancellation: cancellationToken);
    }
}
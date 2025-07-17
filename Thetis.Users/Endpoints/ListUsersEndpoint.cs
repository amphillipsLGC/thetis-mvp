using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Users.Application.Models;
using Thetis.Users.Application.Services;

namespace Thetis.Users.Endpoints;

public class ListUsersResponse
{
    public List<UserModel> Users { get; set; } = [];
}

internal class ListUsersEndpoint(IUserService userService) : EndpointWithoutRequest<ListUsersResponse>
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
        var users = await userService.GetAllUsersAsync(cancellationToken);
        
        await SendOkAsync(new ListUsersResponse
        {
            Users = users
        }, cancellation: cancellationToken);
    }
}
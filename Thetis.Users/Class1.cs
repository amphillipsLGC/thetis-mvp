using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Thetis.Users;

internal interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken);
}

internal class UserService : IUserService
{
    private readonly List<User> _users =
    [
        new User(Guid.CreateVersion7(), "Alice"),
        new User(Guid.CreateVersion7(), "Bob"),
        new User(Guid.CreateVersion7(), "Charlie")
    ];

    public Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<User>>(_users);
    }
}

public record User(Guid Id, string Name);

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/users", async (IUserService userService) =>
        {
            var users = await userService.GetAllUsersAsync(CancellationToken.None);
            return Results.Ok(users);
        });
    }
}

public static class UserServiceExtensions
{
    public static void AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
    }
}
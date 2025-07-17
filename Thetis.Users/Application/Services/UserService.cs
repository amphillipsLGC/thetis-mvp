using Thetis.Users.Application.Models;

namespace Thetis.Users.Application.Services;

internal interface IUserService
{
    Task<List<UserModel>> GetAllUsersAsync(CancellationToken cancellationToken);
}

internal class UserService : IUserService
{
    private readonly List<UserModel> _users =
    [
        new UserModel(Guid.CreateVersion7(), "Alice" , "Smith"),
        new UserModel(Guid.CreateVersion7(), "Bob", "Johnson"),
        new UserModel(Guid.CreateVersion7(), "Charlie", "Brown"),
    ];

    public Task<List<UserModel>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_users);
    }
}
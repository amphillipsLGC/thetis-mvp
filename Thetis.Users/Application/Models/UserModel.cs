using Thetis.Users.Domain;

namespace Thetis.Users.Application.Models;

internal record CreateUserModel(
    string FirstName, 
    string LastName, 
    string? Username, 
    string? Email, 
    string? Password,
    bool EmailVerified, 
    List<UserRoleModel>? Roles
);

internal record UserModel(
    Guid Id, 
    string FirstName, 
    string LastName, 
    string? Username, 
    string? Email, 
    bool EmailVerified,
    List<UserRoleModel>? Roles
);

internal record UserRoleModel(Guid Id, string Name);

internal static class UserExtensions
{
    public static UserModel ToModel(this User user)
    {
        return new UserModel(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Username,
            user.Email,
            user.EmailVerified,
            user.Roles.Select(r => new UserRoleModel(r.Id, r.Name)).ToList()
        );
    }
    
    public static User ToEntity(this UserModel model)
    {
        return new User
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Username = model.Username,
            Email = model.Email,
            EmailVerified = model.EmailVerified
        };
    }

    public static User ToEntity(this CreateUserModel model)
    {
        return new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Username = model.Username,
            Email = model.Email,
            EmailVerified = model.EmailVerified
        };
    }
}
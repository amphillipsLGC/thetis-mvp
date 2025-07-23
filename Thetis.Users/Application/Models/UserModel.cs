using Thetis.Users.Domain;

namespace Thetis.Users.Application.Models;

internal record UserModel(
    Guid Id, 
    string FirstName, 
    string LastName, 
    string? Username, 
    string? Email, 
    bool EmailVerified,
    List<UserRoleModel>? Roles
);

internal record UserRoleModel(Guid RoleId, string RoleName);

internal static class UserExtensions
{
    public static UserModel ToModel(this Domain.User user)
    {
        return new UserModel(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Username,
            user.Email,
            user.EmailVerified,
            user.Roles.Select(r => new UserRoleModel(r.RoleId, r.Role.Name)).ToList()
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
}
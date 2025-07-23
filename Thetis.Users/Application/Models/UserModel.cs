using Thetis.Users.Domain;

namespace Thetis.Users.Application.Models;

internal record UserModel(
    Guid Id, 
    string FirstName, 
    string LastName, 
    string? Username, 
    string? Email, 
    bool EmailVerified, 
    DateTimeOffset CreatedOn, 
    DateTimeOffset? UpdatedOn, 
    DateTimeOffset? LastLogin, 
    bool IsDeleted);

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
            user.CreatedOn,
            user.UpdatedOn,
            user.LastLogin,
            user.IsDeleted);
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
            EmailVerified = model.EmailVerified,
            CreatedOn = model.CreatedOn,
            UpdatedOn = model.UpdatedOn,
            LastLogin = model.LastLogin,
            IsDeleted = model.IsDeleted
        };
    }
    
}
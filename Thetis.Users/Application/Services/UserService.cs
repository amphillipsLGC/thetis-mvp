using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Thetis.Common.Exceptions;
using Thetis.Users.Application.Models;
using Thetis.Users.Data;
using Thetis.Users.Domain;

namespace Thetis.Users.Application.Services;

internal interface IUserService
{
    Task<Result<User>> AddUserAsync(UserModel model, CancellationToken cancellationToken = default);
    Task<Result<User>> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<List<User>> GetUsersAsync(string sortBy, int pageNubmer, int pageSize, CancellationToken cancellationToken);
}

internal class UserService(ILogger<UserService> logger, IUserRepository repository) : IUserService
{
    public async Task<Result<User>> AddUserAsync(UserModel model, CancellationToken cancellationToken = default)
    {
        var user = model.ToEntity();
        
        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.CreateVersion7();
        }
        
        // Check if Username is already in use
        if(user.Username is not null && await repository.GetByUsernameAsync(user.Username, noTracking: true, cancellationToken) is not null)
        {
            logger.LogWarning("Username {Username} already exists.", user.Username);
            return new Result<User>(new UsernameAlreadyInUseException(user.Username));
        }
        
        // Check if Email is already in use
        if(user.Email is not null && await repository.GetByEmailAsync(user.Email, noTracking: true, cancellationToken) is not null)
        {
            logger.LogWarning("Email {Email} already exists.", user.Email);
            return new Result<User>(new EmailAlreadyInUseException(user.Email));
        }
        
        // Add roles if any selected
        if (model.Roles is not null && model.Roles.Count > 0)
        {
            foreach (var role in model.Roles)
            {
                // get role from repository
                //user.Roles.Add();
            }
        }

        try
        {
            user.CreatedOn = DateTime.UtcNow;
            
            await repository.AddAsync(user, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("User {UserId} added successfully.", user.Id);
            return new Result<User>(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<Result<User>> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user.Id == Guid.Empty)
        {
            logger.LogWarning("Attempted to update user with empty ID.");
            return new Result<User>(new ArgumentException("User ID cannot be empty.", nameof(user)));
        }

        try
        {
            var existingUser = await repository.GetByIdAsync(user.Id, noTracking: false, cancellationToken);
            
            if (existingUser is null)
            {
                logger.LogInformation("User {UserId} not found for update.", user.Id);
                return new Result<User>(new EntityNotFoundException("User", user.Id));
            }
            
            // Update the existing user with the new values
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.UpdatedOn = user.UpdatedOn;
            
            // Check if Username has changed
            if(user.Username != null && !(user.Username.Equals(existingUser.Username, StringComparison.OrdinalIgnoreCase)))
            {
                //check if the username already exists
                var usernameExists =
                    await repository.GetByUsernameAsync(user.Username, noTracking: true, cancellationToken) is not null;
                
                if (usernameExists)
                {
                    logger.LogWarning("Username {Username} already exists.", user.Username);
                    return new Result<User>(new ArgumentException("Username is already in use.", user.Username));
                }
                
                existingUser.Username = user.Username;
            }
            
            // Check if Email has changed
            if (user.Email != null && !(user.Email.Equals(existingUser.Email, StringComparison.OrdinalIgnoreCase)))
            {
                //check if the email already exists
                var emailExists =
                    await repository.GetByEmailAsync(user.Email, noTracking: true, cancellationToken) is not null;
                
                if (emailExists)
                {
                    logger.LogWarning("Email {Email} already exists.", user.Email);
                    return new Result<User>(new ArgumentException("Email is already in use.", user.Email));
                }
                
                existingUser.Email = user.Email;
            }
            
            //TODO: Handle changes to roles
            
            await repository.Update(user);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("User {UserId} updated successfully.", user.Id);
            
            return new Result<User>(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user {UserId}", user.Id);
            return new Result<User>(ex);
        }
    }

    public async Task<List<User>> GetUsersAsync(string sortBy, int pageNubmer, int pageSize, CancellationToken cancellationToken)
    {
        if (pageNubmer <= 0 || pageSize <= 0)
        {
            logger.LogWarning("Invalid pagination parameters: pageNumber={PageNumber}, pageSize={PageSize}", pageNubmer, pageSize);
            throw new ArgumentException("Page number and page size must be greater than zero.", nameof(pageNubmer));
        }

        var users = await repository.ListAsync(sortBy, pageNubmer, pageSize, cancellationToken);
        return users;
        
    }
}
using System.Diagnostics;
using System.Text.Json;
using FastEndpoints;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Thetis.Common.Exceptions;
using Thetis.Common.SerDes;
using Thetis.Users.Application.Models;
using Thetis.Users.Data;
using Thetis.Users.Domain;

namespace Thetis.Users.Application.Services;

internal interface IUserService
{
    Task<Result<User>> AddUserAsync(UserModel model, CancellationToken cancellationToken = default);
    Task<Result<User>> UpdateUserAsync(UserModel user, CancellationToken cancellationToken = default);
    Task<List<User>> GetUsersAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

internal class UserService(ILogger<UserService> logger, IUserRepository repository, IRoleRepository roleRepository) : IUserService
{
    public async Task<Result<User>> AddUserAsync(UserModel model, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("UserService.AddUserAsync");
        
        var user = model.ToEntity();
        
        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.CreateVersion7();
        }

        try
        {
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
                    var roleEntity = await roleRepository.GetByIdAsync(role.RoleId, noTracking: true, cancellationToken);
                    
                    if (roleEntity is null)
                    {
                        logger.LogWarning("Role {RoleId} not found for user {UserId}.", role.RoleId, user.Id);
                        continue;
                    }
                    
                    user.Roles.Add(new UserRole { Role = roleEntity });
                }
            }
            
            user.CreatedOn = DateTime.UtcNow;
            
            await repository.AddAsync(user, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("User {UserId} added successfully.", user.Id);
            return new Result<User>(user);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("user", JsonSerializer.Serialize(model, ThetisSerializerOptions.PreserveReferenceHandler));
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            activity?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to add user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<Result<User>> UpdateUserAsync(UserModel user, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("UserService.UpdateUserAsync");
        
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
            existingUser.EmailVerified = user.EmailVerified;
            existingUser.UpdatedOn = DateTimeOffset.UtcNow;
            
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
            
            // Update roles if any selected
            var rolesToRemove = existingUser.Roles
                .Where(r => user.Roles?.All(ur => ur.RoleId != r.RoleId) ?? true)
                .ToList();
            
            if (user.Roles is not null)
            {
                foreach (var role in user.Roles)
                {
                    if (existingUser.Roles.Any(r => r.RoleId == role.RoleId))
                        continue;
                    
                    var roleEntity = await roleRepository.GetByIdAsync(role.RoleId, noTracking: true, cancellationToken);
                    if (roleEntity is null)
                    {
                        logger.LogWarning("Role {RoleId} not found for user {UserId}.", role.RoleId, user.Id);
                        continue;
                    }
                    
                    existingUser.Roles.Add(new UserRole { Role = roleEntity });
                }
            }
            
            // Remove roles that are no longer assigned
            foreach (var role in rolesToRemove)
            {
                existingUser.Roles.Remove(role);
            }
            
            //await repository.Update(existingUser);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("User {UserId} updated successfully.", user.Id);
            
            return new Result<User>(existingUser);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("user", JsonSerializer.Serialize(user, ThetisSerializerOptions.PreserveReferenceHandler));
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to update user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<List<User>> GetUsersAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        using var activity = Activity.Current?.Source.StartActivity("UserService.GetUsersAsync");
        
        if (pageNumber <= 0 || pageSize <= 0)
        {
            logger.LogWarning("Invalid pagination parameters: pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);
            throw new ArgumentException("Page number and page size must be greater than zero.", nameof(pageNumber));
        }

        try
        {
            var users = await repository.ListAsync(sortBy, pageNumber, pageSize, cancellationToken);
            return users;
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("search.filters", JsonSerializer.Serialize(new { sortBy, pageNumber, pageSize }, ThetisSerializerOptions.PreserveReferenceHandler));
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to retrieve users with sortBy={SortBy}, pageNumber={PageNumber}, pageSize={PageSize}", sortBy, pageNumber, pageSize);
            throw;
        }
        
    }
}
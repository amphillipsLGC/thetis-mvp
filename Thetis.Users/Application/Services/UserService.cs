using System.Diagnostics;
using System.Text.Json;
using LanguageExt.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Thetis.Common.Exceptions;
using Thetis.Common.SerDes;
using Thetis.Users.Application.Models;
using Thetis.Users.Data;
using Thetis.Users.Domain;

namespace Thetis.Users.Application.Services;

internal interface IUserService
{
    Task<Result<User>> CreateUserAsync(CreateUserModel model, CancellationToken cancellationToken = default);
    Task<Result<User>> UpdateUserAsync(UserModel user, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<User>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<User>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<User>> GetUsersAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Result<User>> AuthenticateUserAsync(string username, string password, CancellationToken cancellationToken = default);
}

internal class UserService(ILogger<UserService> logger, PasswordHasher hasher, IUserRepository repository, IRoleRepository roleRepository) : IUserService
{
    public async Task<Result<User>> CreateUserAsync(CreateUserModel model, CancellationToken cancellationToken = default)
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
            
            // Check if Password is provided
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                // Hash the password
                user.PasswordHash = hasher.HashPassword(user, model.Password);
            }

            // Add roles if any selected
            if (model.Roles is not null && model.Roles.Count > 0)
            {
                foreach (var role in model.Roles)
                {
                    var roleEntity = await roleRepository.GetByIdAsync(role.Id, noTracking: true, cancellationToken);
                    
                    if (roleEntity is null)
                    {
                        logger.LogWarning("Role {RoleId} not found for user {UserId}.", role.Id, user.Id);
                        continue;
                    }
                    
                    user.Roles.Add(roleEntity);
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
                .Where(r => user.Roles is null || user.Roles.All(ur => ur.Id != r.Id))
                .ToList();
            
            if (user.Roles is not null)
            {
                foreach (var role in user.Roles)
                {
                    if (existingUser.Roles.Any(r => r.Id == role.Id))
                        continue;
                    
                    var roleEntity = await roleRepository.GetByIdAsync(role.Id, noTracking: true, cancellationToken);
                    if (roleEntity is null)
                    {
                        logger.LogWarning("Role {RoleId} not found for user {UserId}.", role.Id, user.Id);
                        continue;
                    }
                    
                    existingUser.Roles.Add(roleEntity);
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

    public async Task<Result<bool>> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("UserService.DeleteUserAsync");
        
        if (userId == Guid.Empty)
        {
            logger.LogWarning("Attempted to delete user with empty ID.");
            return new Result<bool>(new ArgumentException("User ID cannot be empty.", nameof(userId)));
        }

        try
        {
            var user = await repository.GetByIdAsync(userId, noTracking: false, cancellationToken);
            
            if (user is null)
            {
                logger.LogWarning("User {UserId} not found for deletion.", userId);
                return new Result<bool>(new EntityNotFoundException("User", userId));
            }
            
            await repository.Delete(user);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("User {UserId} deleted successfully.", user.Id);
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("user.id", userId.ToString());
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to delete user {UserId}", userId);
            throw;
        }
    }

    public async Task<Result<User>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("UserService.GetUserByIdAsync");
        
        if (userId == Guid.Empty)
        {
            logger.LogWarning("Attempted to get user with empty ID.");
            return new Result<User>(new ArgumentException("User ID cannot be empty.", nameof(userId)));
        }

        try
        {
            var user = await repository.GetByIdAsync(userId, noTracking: true, cancellationToken);

            if (user is null)
            {
                logger.LogWarning("User with ID {UserId} not found.", userId);
                return new Result<User>(new EntityNotFoundException("User", userId));
            }
        
            return new Result<User>(user);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("user.id", userId.ToString());
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to retrieve user by ID {UserId}", userId);
            throw;
        }
    }

    public async Task<Result<User>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("UserService.GetUserByEmailAsync");
        
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning("Attempted to get user with empty email.");
            return new Result<User>(new ArgumentException("Email cannot be empty.", nameof(email)));
        }

        try
        {
            var user = await repository.GetByEmailAsync(email, noTracking: true, cancellationToken);

            if (user is null)
            {
                logger.LogWarning("User with email {Email} not found.", email);
                return new Result<User>(new EntityNotFoundException("User", email));
            }
        
            return new Result<User>(user);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("email", email);
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to retrieve user by email {Email}", email);
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

    public async Task<Result<User>> AuthenticateUserAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("UserService.AuthenticateUserAsync");
        
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Username or password cannot be empty.");
            return new Result<User>(new ArgumentException("Username and password cannot be empty."));
        }

        try
        {
            var user = await repository.GetByUsernameAsync(username, noTracking: true, cancellationToken);
            
            if (user is null)
            {
                logger.LogWarning("User with username {Username} not found.", username);
                return new Result<User>(new UnauthorizedAccessException("Invalid username or password."));
            }
            
            if(user.PasswordHash is null)
            {
                logger.LogWarning("User {Username} does not have a password set.", username);
                return new Result<User>(new UnauthorizedAccessException("User does not have a password set."));
            }
            
            // Verify the password
            var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            
            if (verificationResult != PasswordVerificationResult.Success)
            {
                logger.LogWarning("Invalid password for user {Username}.", username);
                return new Result<User>(new UnauthorizedAccessException("Invalid username or password."));
            }
            
            return new Result<User>(user);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("username", username);
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to authenticate user {Username}", username);
            throw;
        }
    }
}
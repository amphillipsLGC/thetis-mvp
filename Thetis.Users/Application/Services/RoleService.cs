using System.Diagnostics;
using System.Text.Json;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Thetis.Common.Exceptions;
using Thetis.Common.SerDes;
using Thetis.Users.Application.Models;
using Thetis.Users.Data;
using Thetis.Users.Domain;

namespace Thetis.Users.Application.Services;

internal interface IRoleService
{
    Task<Result<Role>> AddRoleAsync(RoleModel model, CancellationToken cancellationToken = default);
    Task<Result<Role>> UpdateRoleAsync(RoleModel role, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Result<Role>> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Result<Role>> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);
    Task<List<Role>> GetRolesAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

internal class RoleService(ILogger<RoleService> logger, IRoleRepository repository) : IRoleService
{
    public async Task<Result<Role>> AddRoleAsync(RoleModel model, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("Users.RoleService.AddRoleAsync");
        
        var role = model.ToEntity();
        
        if (role.Id == Guid.Empty)
        {
            role.Id = Guid.CreateVersion7();
        }
        
        try
        {
            // Check if Role Name is already in use
            if (await repository.GetByNameAsync(role.Name, noTracking: true, cancellationToken) is not null)
            {
                logger.LogWarning("Role name {RoleName} already exists.", role.Name);
                return new Result<Role>(new RoleNameAlreadyExistsException(role.Name));
            }
            
            // Set claims if any provided
            if (model.Claims is not null && model.Claims.Count > 0)
            {
                role.Claims = model.Claims.Select(c => new RoleClaim
                {
                    Id = Guid.CreateVersion7(),
                    ClaimType = c.ClaimType,
                    ClaimValue = c.ClaimValue
                }).ToList();
            }
            
            await repository.AddAsync(role, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Role {RoleId} added successfully.", role.Id);
            return new Result<Role>(role);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("role", JsonSerializer.Serialize(role, ThetisSerializerOptions.PreserveReferenceHandler));
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to add role {RoleName}.", role.Name);
            throw;
        }
    }

    public async Task<Result<Role>> UpdateRoleAsync(RoleModel role, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("Users.RoleService.UpdateRoleAsync");
        
        if (role.Id == Guid.Empty)
        {
            return new Result<Role>(new ArgumentException("Role ID cannot be empty.", nameof(role)));
        }
        
        try
        {
            var existingRole = await repository.GetByIdAsync(role.Id, noTracking: false, cancellationToken);
            
            if (existingRole is null)
            {
                logger.LogWarning("Role with ID {RoleId} not found.", role.Id);
                return new Result<Role>(new EntityNotFoundException("Role", role.Id));
            }
            
            // Update existing role with the new values
            existingRole.Description = role.Description;

            if (!role.Name.Equals(existingRole.Name, StringComparison.OrdinalIgnoreCase))
            {
                // Check if the new role name is already in use
                if (await repository.GetByNameAsync(role.Name, noTracking: true, cancellationToken) is not null)
                {
                    logger.LogWarning("Role name {RoleName} already exists.", role.Name);
                    return new Result<Role>(new RoleNameAlreadyExistsException(role.Name));
                }
                
                existingRole.Name = role.Name;
            }
            
            // Update claims
            if (role.Claims is not null)
            {
                // Add new claims
                foreach (var claim in role.Claims)
                {
                    if (existingRole.Claims.All(c => c.ClaimValue != claim.ClaimValue))
                    {
                        var newClaim = new RoleClaim
                        {
                            Id = Guid.CreateVersion7(),
                            ClaimType = claim.ClaimType,
                            ClaimValue = claim.ClaimValue,
                            RoleId = existingRole.Id
                        };
                        existingRole.Claims.Add(newClaim);
                        
                        // Set the state to Added for EF Core tracking, this is due to 
                        // issues with the change tracker not recognizing new claims
                        repository.DbContext.Entry(newClaim)
                            .State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    }
                }

                // Remove claims that are no longer present
                var claimsToRemove = existingRole.Claims
                    .Where(c => role.Claims.All(rc => rc.ClaimValue != c.ClaimValue))
                    .ToList();

                foreach (var claim in claimsToRemove)
                {
                    existingRole.Claims.Remove(claim);
                }
            }

            //await repository.Update(existingRole);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Role {RoleId} updated successfully.", existingRole.Id);
            
            return new Result<Role>(existingRole);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("role", JsonSerializer.Serialize(role, ThetisSerializerOptions.PreserveReferenceHandler));
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to update role {RoleName}.", role.Name);
            throw;
        }
    }

    public async Task<Result<bool>> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("Users.RoleService.DeleteRoleAsync");
        
        if (roleId == Guid.Empty)
        {
            logger.LogWarning("Attempted to delete a role with an empty ID.");
            return new Result<bool>(new ArgumentException("Role ID cannot be empty.", nameof(roleId)));
        }

        try
        {
            var role = await repository.GetByIdAsync(roleId, noTracking: false, cancellationToken);
        
            if (role is null)
            {
                logger.LogWarning("Role with ID {RoleId} not found.", roleId);
                return new Result<bool>(new EntityNotFoundException("Role", roleId));
            }
            
            await repository.Delete(role);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Role {RoleId} deleted successfully.", roleId);
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("role.id", roleId.ToString());
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to delete role {RoleName}.", roleId);
            throw;
        }
    }

    public async Task<Result<Role>> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("Users.RoleService.GetRoleByIdAsync");
        
        if (roleId == Guid.Empty)
        {
            logger.LogWarning("Attempted to get a role with an empty ID.");
            return new Result<Role>(new ArgumentException("Role ID cannot be empty.", nameof(roleId)));
        }

        try
        {
            var role = await repository.GetByIdAsync(roleId, noTracking: true, cancellationToken);
        
            if (role is null)
            {
                logger.LogWarning("Role with ID {RoleId} not found.", roleId);
                return new Result<Role>(new EntityNotFoundException("Role", roleId));
            }
        
            return new Result<Role>(role);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("role.id", roleId.ToString());
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to retrieve role by ID {RoleId}.", roleId);
            throw;
        }
    }

    public async Task<Result<Role>> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("Users.RoleService.GetRoleByNameAsync");
        
        if (string.IsNullOrWhiteSpace(roleName))
        {
            logger.LogWarning("Attempted to get a role with an empty name.");
            return new Result<Role>(new ArgumentException("Role name cannot be empty.", nameof(roleName)));
        }

        try
        {
            var role = await repository.GetByNameAsync(roleName, noTracking: true, cancellationToken);
        
            if (role is null)
            {
                logger.LogWarning("Role with name {RoleName} not found.", roleName);
                return new Result<Role>(new EntityNotFoundException("Role", roleName));
            }
        
            return new Result<Role>(role);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("role.name", roleName);
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to retrieve role by name {RoleName}.", roleName);
            throw;
        }
    }

    public async Task<List<Role>> GetRolesAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        using var activity = Activity.Current?.Source.StartActivity("Users.RoleService.GetRolesAsync");
        
        if(pageNumber <= 0 || pageSize <= 0)
        {
            logger.LogWarning("Invalid pagination parameters: pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);
            throw new ArgumentException("Page number and page size must be greater than zero.", nameof(pageNumber));
        }

        try
        {
            var roles = await repository.ListAsync(sortBy, pageNumber, pageSize, cancellationToken);
            return roles;
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
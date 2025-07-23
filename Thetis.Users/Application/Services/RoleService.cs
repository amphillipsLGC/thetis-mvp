using System.Diagnostics;
using System.Text.Json;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Thetis.Common.Exceptions;
using Thetis.Users.Application.Models;
using Thetis.Users.Data;
using Thetis.Users.Domain;

namespace Thetis.Users.Application.Services;

internal interface IRoleService
{
    Task<Result<Role>> AddRoleAsync(RoleModel model, CancellationToken cancellationToken = default);
    Task<Result<Role>> UpdateRoleAsync(RoleModel role, CancellationToken cancellationToken = default);
    Task<List<Role>> GetRolesAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken);
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
            
            await repository.AddAsync(role, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Role {RoleId} added successfully.", role.Id);
            return new Result<Role>(role);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("Role", JsonSerializer.Serialize(role));
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
        
        var entity = role.ToEntity();
        
        if (entity.Id == Guid.Empty)
        {
            return new Result<Role>(new ArgumentException("Role ID cannot be empty.", nameof(role)));
        }

        try
        {
            var existingRole = await repository.GetByIdAsync(entity.Id, noTracking: true, cancellationToken);
            
            if (existingRole is null)
            {
                logger.LogWarning("Role with ID {RoleId} not found.", entity.Id);
                return new Result<Role>(new EntityNotFoundException("Role", entity.Id));
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
            
            // Update claims if provided
            if (role.Claims is not null)
            {
                //TODO: Handle claims update logic here
            }

            await repository.Update(entity);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Role {RoleId} updated successfully.", entity.Id);
            
            return new Result<Role>(entity);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddTag("Role", JsonSerializer.Serialize(entity));
            Activity.Current?.AddTag("exception", ex.Message);
            Activity.Current?.AddTag("stacktrace", ex.StackTrace);
            Activity.Current?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Failed to update role {RoleName}.", entity.Name);
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
        
        var roles = await repository.ListAsync(sortBy, pageNumber, pageSize, cancellationToken);
        return roles;
    }
}
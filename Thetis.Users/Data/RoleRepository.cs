using Microsoft.EntityFrameworkCore;
using Thetis.Users.Domain;

namespace Thetis.Users.Data;

internal interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid roleId, bool noTracking = false, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string roleName, bool noTracking = false, CancellationToken cancellationToken = default);
    Task<List<Role>> ListAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(Role role, CancellationToken cancellationToken);
    Task Update(Role role);
    Task Delete(Role role);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

internal class RoleRepository(UserDbContext dbContext) : IRoleRepository
{
    
    public async Task<Role?> GetByIdAsync(Guid roleId, bool noTracking = false, CancellationToken cancellationToken = default)
    {
        var query = noTracking
            ? dbContext.Roles.AsNoTracking()
            : dbContext.Roles;

        return await query
            .Include(i => i.Claims)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string roleName, bool noTracking = false, CancellationToken cancellationToken = default)
    {
        var query = noTracking
            ? dbContext.Roles.AsNoTracking()
            : dbContext.Roles;

        return await query
            .Include(i => i.Claims)
            .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower(), cancellationToken);
    }

    public async Task<List<Role>> ListAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Roles.Include(i => i.Claims).AsQueryable();

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortBy.ToLower() switch
            {
                "name" => query.OrderBy(r => r.Name),
                _ => query.OrderBy(r => r.Id)
            };
        }
        
        var list = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return list;
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken)
    {
        await dbContext.Roles.AddAsync(role, cancellationToken);
    }

    public Task Update(Role role)
    {
        dbContext.Roles.Update(role);
        return Task.CompletedTask;
    }

    public Task Delete(Role role)
    {
        dbContext.Roles.Remove(role);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
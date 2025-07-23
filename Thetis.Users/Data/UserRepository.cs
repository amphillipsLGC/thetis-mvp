using Microsoft.EntityFrameworkCore;
using Thetis.Users.Domain;

namespace Thetis.Users.Data;

internal interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, bool noTracking, CancellationToken cancellationToken);
    Task<User?> GetByUsernameAsync(string username, bool noTracking, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, bool noTracking, CancellationToken cancellationToken);
    Task<List<User>> ListAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task Update(User user);
    Task Delete(User user);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

internal class UserRepository(UserDbContext dbContext): IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid userId, bool noTracking = false,  CancellationToken cancellationToken = default)
    {
        var query = noTracking
            ? dbContext.Users.AsNoTracking()
            : dbContext.Users;
        
        return await query
            .Include(i => i.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, bool noTracking = false, CancellationToken cancellationToken = default)
    {
        var query = noTracking
            ? dbContext.Users.AsNoTracking()
            : dbContext.Users;

        return query
            .Include(i => i.Roles)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, bool noTracking = false, CancellationToken cancellationToken = default)
    {
        var query = noTracking
            ? dbContext.Users.AsNoTracking()
            : dbContext.Users;

        return await query
            .Include(i => i.Roles)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<List<User>> ListAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Users
            .Include(i => i.Roles)
            .AsQueryable();

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortBy.ToLower() switch
            {
                "firstname" => query.OrderBy(u => u.FirstName),
                "lastname" => query.OrderBy(u => u.LastName),
                "email" => query.OrderBy(u => u.Email),
                _ => query.OrderBy(u => u.CreatedOn)
            };
        }

        // Apply pagination
        var list = await query.Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

        return list;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task Update(User user)
    {
        dbContext.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task Delete(User user)
    {
        dbContext.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
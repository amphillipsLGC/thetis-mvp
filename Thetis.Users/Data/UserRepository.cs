using Microsoft.EntityFrameworkCore;
using Thetis.Users.Domain;

namespace Thetis.Users.Data;

internal interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, bool noTracking, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, bool noTracking, CancellationToken cancellationToken);

    Task<List<User>> ListAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
    Task DeleteAsync(User user, CancellationToken cancellationToken);
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
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, bool noTracking = false, CancellationToken cancellationToken = default)
    {
        var query = noTracking
            ? dbContext.Users.AsNoTracking()
            : dbContext.Users;

        return query
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<List<User>> ListAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Users.AsQueryable();

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
        return query.Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Thetis.Users.Domain;

namespace Thetis.Users.Data;

internal interface IUserRepository
{
    UserDbContext DbContext { get; }
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
    public UserDbContext DbContext => dbContext;
    
    public async Task<User?> GetByIdAsync(Guid userId, bool noTracking = false,  CancellationToken cancellationToken = default)
    {
        var query = noTracking
            ? dbContext.Users.AsNoTracking()
            : dbContext.Users;
        
        var user = await query
            .Include(i => i.Roles)
            .Where(u => u.Id == userId)
            .Select(u =>  new User
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.Username,
                Email = u.Email,
                EmailVerified = u.EmailVerified,
                PasswordHash = u.PasswordHash,
                CreatedOn = u.CreatedOn,
                UpdatedOn = u.UpdatedOn,
                LastLogin = u.LastLogin,
                IsDeleted = u.IsDeleted,
                Roles = u.Roles.Select(r => new Role
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Claims = r.Claims
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        
        return user;
    }

    public Task<User?> GetByUsernameAsync(string username, bool noTracking = false, CancellationToken cancellationToken = default)
    {
        var query = noTracking
            ? dbContext.Users.AsNoTracking()
            : dbContext.Users;

        var user = query
            .Include(i => i.Roles)
            .Where(u => u.Username == username)
            .Select(u =>  new User
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.Username,
                Email = u.Email,
                EmailVerified = u.EmailVerified,
                PasswordHash = u.PasswordHash,
                CreatedOn = u.CreatedOn,
                UpdatedOn = u.UpdatedOn,
                LastLogin = u.LastLogin,
                IsDeleted = u.IsDeleted,
                Roles = u.Roles.Select(r => new Role
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Claims = r.Claims
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email, bool noTracking = false, CancellationToken cancellationToken = default)
    {
        var query = noTracking
            ? dbContext.Users.AsNoTracking()
            : dbContext.Users;

        var user = await query
            .Include(i => i.Roles)
            .Where(u => u.Email == email)
            .Select(u =>  new User
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.Username,
                Email = u.Email,
                EmailVerified = u.EmailVerified,
                PasswordHash = u.PasswordHash,
                CreatedOn = u.CreatedOn,
                UpdatedOn = u.UpdatedOn,
                LastLogin = u.LastLogin,
                IsDeleted = u.IsDeleted,
                Roles = u.Roles.Select(r => new Role
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Claims = r.Claims
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user;
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
                    .Select(u =>  new User
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Username = u.Username,
                        Email = u.Email,
                        EmailVerified = u.EmailVerified,
                        PasswordHash = u.PasswordHash,
                        CreatedOn = u.CreatedOn,
                        UpdatedOn = u.UpdatedOn,
                        LastLogin = u.LastLogin,
                        IsDeleted = u.IsDeleted,
                        Roles = u.Roles.Select(r => new Role
                        {
                            Id = r.Id,
                            Name = r.Name,
                            Description = r.Description,
                            Claims = r.Claims
                        }).ToList()
                    })
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
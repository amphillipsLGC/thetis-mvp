using Microsoft.EntityFrameworkCore;
using Thetis.Profiles.Domain;

namespace Thetis.Profiles.Data;

internal interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(Guid profileId, bool noTracking, CancellationToken cancellationToken);
    Task<List<Profile>> ListAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(Profile profile, CancellationToken cancellationToken);
    Task Update(Profile profile);
    Task Delete(Profile profile);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

internal class ProfileRepository(ProfileDbContext dbContext) : IProfileRepository
{
    public async Task<Profile?> GetByIdAsync(Guid profileId, bool noTracking = false, CancellationToken cancellationToken = default)
    {
        var query = noTracking
            ? dbContext.Profiles.AsNoTracking()
            : dbContext.Profiles;
        
        return await query.FirstOrDefaultAsync(x => x.Id == profileId, cancellationToken);
    }

    public async Task<List<Profile>> ListAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Profiles.AsNoTracking().AsQueryable();

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortBy.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),
                "createdOn" => query.OrderBy(p => p.CreatedOn),
                _ => query.OrderBy(p => p.Id)
            };
        }

        // Apply pagination
        var list = await query.Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
        
        return list;
    }

    public async Task AddAsync(Profile profile, CancellationToken cancellationToken)
    {
        await dbContext.Profiles.AddAsync(profile, cancellationToken);
    }

    public Task Update(Profile profile)
    {
        dbContext.Profiles.Update(profile);
        return Task.CompletedTask;
    }

    public Task Delete(Profile profile)
    {
        dbContext.Profiles.Remove(profile);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
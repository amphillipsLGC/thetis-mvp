using Microsoft.Extensions.Logging;
using Thetis.Profiles.Data;
using Thetis.Profiles.Domain;

namespace Thetis.Profiles.Application.Services;

internal interface IProfileService
{
    Task<Profile?> GetProfileByIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<List<Profile>> GetUserProfilesAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task AddProfileAsync(Profile profile, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(Profile profile, CancellationToken cancellationToken = default);
    Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
}

internal class ProfileService(ILogger<ProfileService> logger, IProfileRepository repository) : IProfileService
{
    public async Task<Profile?> GetProfileByIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        if (profileId == Guid.Empty)
        {
            logger.LogWarning("Attempted to get profile with empty ID.");
            return null;
        }
        
        var profile = await repository.GetByIdAsync(profileId, noTracking: true, cancellationToken);

        if (profile is null)
        {
            logger.LogInformation("Profile with ID {ProfileId} not found.", profileId);
        }
        
        return profile;
    }

    public async Task<List<Profile>> GetUserProfilesAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber > 0 && pageSize > 0)
            return await repository.ListAsync(sortBy, pageNumber, pageSize, cancellationToken);
        
        logger.LogWarning("Invalid pagination parameters: pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);
        return [];
    }

    //TODO: implement result patterns for better error handling
    public Task AddProfileAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        if (profile is null)
        {
            logger.LogWarning("Attempted to add a null profile.");
            throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
        }

        if (profile.Id == Guid.Empty)
        {
            profile.Id = Guid.NewGuid(); // Ensure profile has a valid ID
        }

        return repository.AddAsync(profile, cancellationToken);
    }

    public Task UpdateProfileAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        if (profile is null)
        {
            logger.LogWarning("Attempted to update a null profile.");
            throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
        }

        if (profile.Id == Guid.Empty)
        {
            logger.LogWarning("Attempted to update a profile with an empty ID.");
            throw new ArgumentException("Profile ID cannot be empty.", nameof(profile));
        }

        return repository.Update(profile);
    }

    public async Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        if (profileId == Guid.Empty)
        {
            logger.LogWarning("Attempted to delete a profile with an empty ID.");
            throw new ArgumentException("Profile ID cannot be empty.", nameof(profileId));
        }

        var profile = await repository.GetByIdAsync(profileId, noTracking: false, cancellationToken);
        if (profile is null)
        {
            logger.LogInformation("Profile with ID {ProfileId} not found.", profileId);
            return;
        }
        
        await repository.Delete(profile);
        
    }
}
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Thetis.Common.Exceptions;
using Thetis.Profiles.Data;
using Thetis.Profiles.Domain;

namespace Thetis.Profiles.Application.Services;

internal interface IProfileService
{
    Task<Profile?> GetProfileByIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<List<Profile>> GetUserProfilesAsync(string sortBy, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<Profile>> AddProfileAsync(Profile profile, CancellationToken cancellationToken = default);
    Task<Result<Profile>> UpdateProfileAsync(Profile profile, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
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
            logger.LogInformation("Profile {ProfileId} not found.", profileId);
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
    
    public  async Task<Result<Profile>> AddProfileAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        if (profile.Id == Guid.Empty)
        {
            profile.Id = Guid.CreateVersion7(); // Ensure profile has a valid ID
        }

        try
        {
            await repository.AddAsync(profile, cancellationToken);
            
            logger.LogInformation("Profile {ProfileId} added successfully.", profile.Id);
            return new Result<Profile>(profile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding profile {ProfileId}", profile.Id);
            return new Result<Profile>(ex);
        }
        
    }

    public async Task<Result<Profile>> UpdateProfileAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        if (profile.Id == Guid.Empty)
        {
            logger.LogWarning("Attempted to update a profile with an empty ID.");
            var ex = new ArgumentException("Profile ID cannot be empty.", nameof(profile));
            return new Result<Profile>(ex);
        }

        try
        {
            var existingProfile = await repository.GetByIdAsync(profile.Id, noTracking: false, cancellationToken);
            
            if (existingProfile is null)
            {
                logger.LogWarning("Profile with ID {ProfileId} not found for update.", profile.Id);
                var ex = new EntityNotFoundException("Profile", profile.Id);
                return new Result<Profile>(ex);
            }
            
            // Update the existing profile with the new values
            existingProfile.Name = profile.Name;
            existingProfile.Description = profile.Description;
            existingProfile.IsPublic = profile.IsPublic;
            existingProfile.DataRequirements = profile.DataRequirements;
            existingProfile.ModifiedOn = DateTimeOffset.UtcNow;
            
            await repository.Update(existingProfile);
            
            await repository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Profile {ProfileId} updated successfully.", profile.Id);
            
            return new Result<Profile>(profile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating profile {ProfileId}", profile.Id);
            return new Result<Profile>(ex);
        }
    }

    public async Task<Result<bool>> DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        if (profileId == Guid.Empty)
        {
            logger.LogWarning("Attempted to delete a profile with an empty ID.");
            var ex = new ArgumentException("Profile ID cannot be empty.", nameof(profileId));
            return new Result<bool>(ex);
        }

        var profile = await repository.GetByIdAsync(profileId, noTracking: false, cancellationToken);
        if (profile is null)
        {
            logger.LogWarning("Profile with ID {ProfileId} not found.", profileId);
            var ex = new KeyNotFoundException($"Profile with ID {profileId} not found.");
            return new Result<bool>(ex);
        }

        try
        {
            await repository.Delete(profile);
            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Profile {ProfileId} deleted successfully.", profileId);
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting profile {ProfileId}", profileId);
            return new Result<bool>(ex);
        }
        
        
    }
}
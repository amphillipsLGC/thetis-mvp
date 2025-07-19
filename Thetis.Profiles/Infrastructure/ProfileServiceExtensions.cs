using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Thetis.Profiles.Infrastructure;

public static class ProfileServiceExtensions
{
    public static void AddProfileServices(this IServiceCollection services, ConfigurationManager config)
    {
        var connectionString = config.GetConnectionString("ProfilesDbConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'ProfilesDbConnectionString' is not configured.");
        }

        // services.AddDbContext<ProfileDbContext>(options =>
        // {
        //     options.UseNpgsql(connectionString, npgsqlOptions =>
        //     {
        //         npgsqlOptions.EnableRetryOnFailure();
        //         npgsqlOptions.CommandTimeout(30);
        //     });
        // });
        //
        // services.AddScoped<IProfileRepository, ProfileRepository>();
        // services.AddScoped<IProfileService, ProfileService>();
    }
}
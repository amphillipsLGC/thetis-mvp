using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Thetis.Users.Application.Services;
using Thetis.Users.Data;

namespace Thetis.Users.Infrastructure;

public static class UserServiceExtensions
{
    public static void AddUserServices(this IServiceCollection services, ConfigurationManager config)
    {
        var connectionString = config.GetConnectionString("UsersDbConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'UsersDbConnectionString' is not configured.");
        }

        services.AddDbContext<UserDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
                npgsqlOptions.CommandTimeout(30);
            });
        });
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
    }
}
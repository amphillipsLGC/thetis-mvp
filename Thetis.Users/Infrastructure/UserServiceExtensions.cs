using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Thetis.Users.Application.Services;
using Thetis.Users.Data;
using Thetis.Users.Domain;

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
        
        services.AddSingleton<PasswordHasher>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();
    }
}
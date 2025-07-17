using Microsoft.Extensions.DependencyInjection;

namespace Thetis.Users;

public static class UserServiceExtensions
{
    public static void AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
    }
}
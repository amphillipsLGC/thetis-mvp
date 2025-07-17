using Microsoft.Extensions.DependencyInjection;
using Thetis.Users.Application.Services;

namespace Thetis.Users.Infrastructure;

public static class UserServiceExtensions
{
    public static void AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
    }
}
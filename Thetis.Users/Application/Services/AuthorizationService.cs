using Microsoft.AspNetCore.Authorization;

namespace Thetis.Users.Application.Services;

internal enum SystemPermissions
{
    CanCreateProfiles,
    CanPublishToFhirServer,
    CanAdministerSystem
}

internal static class SystemPermissionsProvider
{
    public static List<SystemPermissions> GetSystemClaims()
    {
        return Enum.GetValues(typeof(SystemPermissions)).OfType<SystemPermissions>().ToList();
    }
}

internal static class SystemClaims
{
    public const string Subject = "sub";
    public const string Email = "email";
    public const string Roles = "roles";
    public const string Permissions = "permissions";
}

internal static class AuthorizationPolicies
{
    public static AuthorizationPolicy AllowedToCreateProfiles()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(SystemClaims.Permissions, nameof(SystemPermissions.CanCreateProfiles))
            .Build();
    }
    
    public static AuthorizationPolicy AllowedToPublishToFhirServer()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(SystemClaims.Permissions, nameof(SystemPermissions.CanPublishToFhirServer))
            .Build();
    }
    
    public static AuthorizationPolicy AllowedToAdministerSystem()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(SystemClaims.Permissions, nameof(SystemPermissions.CanAdministerSystem))
            .Build();
    }
}
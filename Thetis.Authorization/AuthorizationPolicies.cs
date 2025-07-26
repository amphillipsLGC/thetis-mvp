using Microsoft.AspNetCore.Authorization;

namespace Thetis.Authorization;

public static class AuthorizationPolicies
{
    public static AuthorizationPolicyBuilder AllowedToCreateProfiles()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(SystemClaims.Permission, nameof(SystemPermissions.CanCreateProfiles));
    }
    
    public static AuthorizationPolicyBuilder AllowedToPublishToFhirServer()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(SystemClaims.Permission, nameof(SystemPermissions.CanPublishToFhirServer));
    }
    
    public static AuthorizationPolicyBuilder AllowedToAdministerSystem()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(SystemClaims.Permission, nameof(SystemPermissions.CanAdministerSystem));
    }
}

public enum PolicyNames
{
    ProfileCreator,
    TestDataPublisher,
    SystemAdministrator
}

public static class SystemClaims
{
    public const string Subject = "sub";
    public const string Name = "name";
    public const string Email = "email";
    public const string Roles = "roles";
    public const string Permission = "permission";
    
    // Known JWT claim types
    public const string FamilyName = "family_name";
    public const string GivenName = "given_name";
}

public enum SystemPermissions
{
    CanCreateProfiles,
    CanPublishToFhirServer,
    CanAdministerSystem
}

public static class SystemPermissionsProvider
{
    public static List<SystemPermissions> GetSystemClaims()
    {
        return Enum.GetValues(typeof(SystemPermissions)).OfType<SystemPermissions>().ToList();
    }
}

public static class ThetisAuthenticationSchemes
{
    public  const string Cookie = "cookie";
    public const string Oidc = "oidc";
    public const string Jwt = "jwt";
}
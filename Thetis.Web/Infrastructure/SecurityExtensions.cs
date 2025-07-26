using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Thetis.Authorization;
using Thetis.Common;

namespace Thetis.Web.Infrastructure;

public static class SecurityExtensions
{
    public static void AddSecurity(this WebApplicationBuilder builder, Action<SecurityServiceOptions>? options = null)
    {
        var securityServiceOptions = new SecurityServiceOptions();
        options?.Invoke(securityServiceOptions);
        
        var authBuilder = builder.Services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = ThetisAuthenticationSchemes.Cookie;
        });

        List<string> validAuthSchemes = [ThetisAuthenticationSchemes.Cookie];
        
        // Configure cookie authentication
        authBuilder.AddCookie(ThetisAuthenticationSchemes.Cookie, opts =>
        {
            opts.Cookie.Name = "thetis_cookie";
            opts.Cookie.HttpOnly = true;
            opts.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            opts.Cookie.SameSite = SameSiteMode.Strict;
            opts.LoginPath = "/api/login";
            opts.LogoutPath = "/api/logout";
            
            opts.Events.OnSigningIn = context =>
            {
                //get claims principal
                if (context.Principal?.Identity is not ClaimsIdentity claimsIdentity)
                {
                    return Task.CompletedTask;
                }
                
                // Define the claim types to keep
                var allowedClaims = new HashSet<string> {
                    SystemClaims.FamilyName,
                    SystemClaims.GivenName,
                    SystemClaims.Name,
                    SystemClaims.Email,
                    SystemClaims.Subject,
                    SystemClaims.Roles,
                    SystemClaims.Permission
                };
                
                // Define all claims that should be removed
                var claimsToRemove = claimsIdentity.Claims
                    .Where(claim => !allowedClaims.Contains(claim.Type))
                    .ToList();
                
                // Remove claims
                foreach (var claim in claimsToRemove)
                {
                    claimsIdentity.RemoveClaim(claim);
                }
                
                return Task.CompletedTask;
            };
            
            opts.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.Request.Host + "/api/login");
                return Task.CompletedTask;
            };

            opts.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.Request.Host + "/api/login");
                return Task.CompletedTask;
            };
        });
        
        // Configure Challenge Schemas
        if (builder.Configuration.GetValue<bool>("Authentication:OIDC:Enabled"))
        {
            validAuthSchemes.Add(ThetisAuthenticationSchemes.Oidc);
            
            authBuilder.AddOpenIdConnect(ThetisAuthenticationSchemes.Oidc, opts =>
            {
                opts.Authority = builder.Configuration["Authentication:OIDC:Authority"];
                opts.ClientId = builder.Configuration["Authentication:OIDC:ClientId"];
                opts.ClientSecret = builder.Configuration["Authentication:OIDC:ClientSecret"];
                opts.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                opts.Scope.Add(SystemClaims.Email); // openId and profile are added by default
                opts.SaveTokens = false;
                opts.MapInboundClaims = false;
                opts.ResponseType = "code";
                opts.CallbackPath = "/signin-oidc";

                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = SystemClaims.Name,
                    RoleClaimType = SystemClaims.Roles,
                };

                opts.Events.OnTokenValidated = context =>
                {
                    // Increment user login count
                    ApplicationDiagnostics.UserLoginsCounter.Add(1, new KeyValuePair<string, object?>("authentication.scheme", ThetisAuthenticationSchemes.Oidc));
                
                    return Task.CompletedTask;
                };
                
                opts.Events.OnAuthenticationFailed = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.Request.Host + "/api/login");
                    return Task.CompletedTask;
                };
                
                opts.Events.OnAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.Request.Host + "/api/login");
                    return Task.CompletedTask;
                };
            });
        }
        
        // Configure optional authentication Schemas
        if(builder.Configuration.GetValue<bool>("Authentication:JWT:Enabled"))
        {
            validAuthSchemes.Add(ThetisAuthenticationSchemes.Jwt);
            
            authBuilder.AddJwtBearer(ThetisAuthenticationSchemes.Jwt, opts =>
            {
                opts.Authority = builder.Configuration["Authentication:JWT:Authority"];
                opts.Audience = builder.Configuration["Authentication:JWT:Audience"];
                opts.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                opts.MapInboundClaims = false;
                
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = SystemClaims.Name,
                    RoleClaimType = SystemClaims.Roles,
                    ValidTypes = securityServiceOptions.ValidJwtTypes ?? ["at+jwt", "JWT"],
                };
                
                opts.Events.OnAuthenticationFailed = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.Request.Host + "/api/login");
                    return Task.CompletedTask;
                };

                opts.Events.OnForbidden = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.Request.Host + "/api/login");
                    return Task.CompletedTask;
                };
            });
        }
        
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(nameof(PolicyNames.ProfileCreator), AuthorizationPolicies.AllowedToCreateProfiles()
                .AddAuthenticationSchemes([..validAuthSchemes]).Build())
            .AddPolicy(nameof(PolicyNames.TestDataPublisher), AuthorizationPolicies.AllowedToPublishToFhirServer()
                .AddAuthenticationSchemes([..validAuthSchemes]).Build())
            .AddPolicy(nameof(PolicyNames.SystemAdministrator), AuthorizationPolicies.AllowedToAdministerSystem()
                .AddAuthenticationSchemes([..validAuthSchemes]).Build());
    }
    
    public class SecurityServiceOptions
    {
        public string[]? ValidJwtTypes { get; set; } = ["at+jwt", "JWT"];
    }
}
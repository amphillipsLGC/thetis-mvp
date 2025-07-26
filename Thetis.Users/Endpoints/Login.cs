using System.Diagnostics;
using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Authorization;
using Thetis.Common;
using Thetis.Users.Application.Models;
using Thetis.Users.Application.Services;

namespace Thetis.Users.Endpoints;

internal record LoginRequest(string Username, string Password, bool UseIdentityProvider = false);

internal class Login(IUserService userService) : Endpoint<LoginRequest>
{
    public override void Configure()
    {
        Post("/login");
        Description(x => x
            .WithName("Login")
            .Produces(200)
            .ProducesProblem(401)
            .ProducesProblem(500));
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest request, CancellationToken ct)
    {
        if(request.UseIdentityProvider)
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            await HttpContext.ChallengeAsync(ThetisAuthenticationSchemes.Oidc, properties);
            return;
        }
        
        var user = await userService.AuthenticateUserAsync(request.Username, request.Password, ct);
         
        await user.Match(async success =>
            {
                var authenticatedUser = success.ToModel();
                var claims = new List<Claim>
                {
                    new Claim(SystemClaims.GivenName, authenticatedUser.FirstName),
                    new Claim(SystemClaims.Email, authenticatedUser.Email ?? string.Empty),
                    new Claim(SystemClaims.FamilyName, authenticatedUser.LastName),
                    new Claim(SystemClaims.Subject, authenticatedUser.Id.ToString())
                };
                
                //TODO FIx this to include the claims in the roles, need to add the claims to the user model
                if(authenticatedUser.Roles is not null && authenticatedUser.Roles.Count > 0)
                {
                    claims.AddRange(authenticatedUser.Roles.Select(role => new Claim(SystemClaims.Roles, role.Name)));
                    
                    // Create a unique set of claims
                    var uniqueClaims = authenticatedUser.Roles
                        .SelectMany(role => role.Claims)
                        .GroupBy(claim => claim.ClaimValue)
                        .Select(g => g.First())
                        .ToList();
                    
                    claims.AddRange(uniqueClaims.Select(c => new Claim(c.ClaimType, c.ClaimValue)));
                }
                
                var identity = new ClaimsIdentity(claims, ThetisAuthenticationSchemes.Cookie);
                var principal = new ClaimsPrincipal(identity);
                
                // Increment user login count
                ApplicationDiagnostics.UserLoginsCounter.Add(1, new KeyValuePair<string, object?>("authentication.scheme", ThetisAuthenticationSchemes.Cookie));

                await HttpContext.SignInAsync(
                    ThetisAuthenticationSchemes.Cookie,
                    principal
                );
            },
        error => error switch
            {
                UnauthorizedAccessException => SendAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "Invalid username or password.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier
                }, statusCode: StatusCodes.Status401Unauthorized, cancellation: ct),
                _ => SendAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = $"An unexpected error occurred while logging in. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                }, StatusCodes.Status400BadRequest, cancellation: ct)
            }
        );
        
         
    }
    
}
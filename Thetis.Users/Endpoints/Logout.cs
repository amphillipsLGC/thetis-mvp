using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Authorization;

namespace Thetis.Users.Endpoints;

internal class Logout() : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/logout");
        Description(x => x
            .WithName("Logout")
            .Produces(200)
            .ProducesProblem(401)
            .ProducesProblem(500));
        Policies(nameof(PolicyNames.AuthenticatedUser));
        
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = null,
            AllowRefresh = false
        };
        
        await HttpContext.SignOutAsync(ThetisAuthenticationSchemes.Cookie, props);
        //await HttpContext.SignOutAsync(ThetisAuthenticationSchemes.Oidc, props);
        await SendNoContentAsync(cancellationToken);
    }
    
}
using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Authorization;

namespace Thetis.Users.Endpoints.Users;

internal record UserDetailsModel(Guid Id, string FirstName, string LastName, List<string> Permissions);

internal class UserDetails(IHttpContextAccessor httpContextAccessor) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/users/me");
        Description(x => x
            .WithName("Get user details")
            .Produces<UserDetailsModel>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(500));
        Policies(nameof(PolicyNames.AuthenticatedUser));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Detail = "User is not authenticated."
            }, StatusCodes.Status401Unauthorized, cancellation: cancellationToken);
            return;
        }

        var userId = user.FindFirst(SystemClaims.Subject)?.Value;
        var firstName = user.FindFirst(SystemClaims.GivenName)?.Value;
        var lastName = user.FindFirst(SystemClaims.FamilyName)?.Value;
        var permissions = user.FindAll(SystemClaims.Permission).Select(c => c.Value).ToList();
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "User ID is not available in the claims."
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        var userDetails = new UserDetailsModel(
            Id: Guid.Parse(userId),
            FirstName: firstName ?? string.Empty,
            LastName: lastName ?? string.Empty,
            Permissions: permissions
        );

        await SendAsync(userDetails, StatusCodes.Status200OK, cancellation: cancellationToken);
    }
    
}
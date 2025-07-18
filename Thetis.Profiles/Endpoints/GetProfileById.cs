using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Profiles.Application.Models;
using Thetis.Profiles.Application.Services;

namespace Thetis.Profiles.Endpoints;

internal class GetProfileByIdResponse
{
    public ProfileModel Profile { get; set; } = null!;
}

internal class GetProfileById(IProfileService profileService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/profiles/{profileId}");
        Description(x => x
            .WithName("Get Profile by ID")
            .Produces<ProfileModel>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500));
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        // Extract profileId from the route
        var profileId = Route<string>("profileId");

        // Validate the profileId format
        if(!Guid.TryParse(profileId, out var id))
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid profile ID format.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            };
            
            await SendAsync(problem, StatusCodes.Status400BadRequest, cancellation: cancellationToken);
            return;
        }
        
        if (id == Guid.Empty)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Profile ID cannot be empty.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            };
            
            await SendAsync(problem, StatusCodes.Status400BadRequest, cancellation: cancellationToken);
            return;
      
        }

        // Fetch the profile by ID
        var result = await profileService.GetProfileByIdAsync(id, cancellationToken);

        if (result is not null)
        {
            var response = new GetProfileByIdResponse
            {
                Profile = result.ToModel()
            };
            await SendOkAsync(result, cancellation: cancellationToken);
        }
        else
        {
            await SendNotFoundAsync(cancellation: cancellationToken);
        }
    }
}
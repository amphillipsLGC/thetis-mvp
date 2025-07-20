using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Thetis.Common.Exceptions;
using Thetis.Profiles.Application.Models;
using Thetis.Profiles.Application.Services;

namespace Thetis.Profiles.Endpoints;

internal class UpdateProfile(IProfileService profileService) : Endpoint<ProfileModel, Results<NoContent, ProblemDetails>>
{
    public override void Configure()
    {
        Put("/profiles/{id}");
        Description(x => x
            .WithName("Update an existing profile")
            .Produces<NoContent>(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500));
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(ProfileModel request, CancellationToken cancellationToken)
    {
        var profileId = Route<string>("id");
        
        // Validate the profile ID from the route
        if (string.IsNullOrWhiteSpace(profileId) || !Guid.TryParse(profileId, out var id))
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid profile ID format.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        // Ensure the request body ID matches the route ID
        if(request.Id != id)
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Profile ID in the request body does not match the ID in the route.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        var result = await profileService.UpdateProfileAsync(request.ToEntity(), cancellationToken);
        
        var response = result.Match<IResult>(
            success => TypedResults.NoContent(),
            error => error switch
            {
                EntityNotFoundException _ => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Detail = error.Message,
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                },
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = error.Message,
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                }
            }
        );
        
        switch (response)
        {
            case NoContent noContent:
                await SendAsync(noContent, cancellation: cancellationToken);
                break;
            case ProblemDetails problemDetails:
                await SendAsync(problemDetails, StatusCodes.Status400BadRequest, cancellation: cancellationToken);
                break;
        }
    }
}
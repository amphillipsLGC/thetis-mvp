using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Thetis.Profiles.Application.Models;
using Thetis.Profiles.Application.Services;

namespace Thetis.Profiles.Endpoints;

internal class ListProfilesResponse
{
    public List<ProfileModel> Profiles { get; set; } = [];
}

internal class ListProfiles(IProfileService profileService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/profiles");
        Description(x => x
            .WithName("List all profiles")
            .Produces<ListProfilesResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        // Extract query parameters
        var sortBy = Query<string?>("sortBy", false) ?? "CreatedOn";
        var pageNumber = Query<int?>("pageNumber", false) ?? 1;
        var pageSize = Query<int?>("pageSize", false) ?? 10;
        
        var profiles = await profileService.GetUserProfilesAsync(sortBy, pageNumber, pageSize, cancellationToken);
        
        var response = new ListProfilesResponse
        {
            Profiles = profiles.Select(p => p.ToModel()).ToList()
        };
        
        await SendOkAsync(response, cancellation: cancellationToken);
    }
    
}
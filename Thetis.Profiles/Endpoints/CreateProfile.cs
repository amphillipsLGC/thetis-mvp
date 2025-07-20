using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Thetis.Profiles.Application.Models;
using Thetis.Profiles.Application.Services;

namespace Thetis.Profiles.Endpoints;

internal class CreateProfile(IProfileService profileService) : Endpoint<ProfileModel, Results<Created<ProfileModel>, ProblemDetails>>
{
    public override void Configure()
    {
        Post("/profiles");
        Description(x => x
            .WithName("Create a new profile")
            .Produces<ProfileModel>(201)
            .ProducesProblem(400)
            .ProducesProblem(500));
        AllowAnonymous();
    }

    public override async Task HandleAsync(ProfileModel request, CancellationToken cancellationToken)
    {
        var result = await profileService.AddProfileAsync(request.ToEntity(), cancellationToken);
        
        var response = result.Match<IResult>(
            success => TypedResults.Created($"/profiles/{success.Id}", success.ToModel()),
            error => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = $"An unexpected error occurred while deleting the profile. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }
        );
        
        switch (response)
        {
            case Created<ProfileModel> createdResponse:
                await SendAsync(createdResponse, cancellation: cancellationToken);
                break;
            case ProblemDetails problemDetails:
                await SendAsync(problemDetails, StatusCodes.Status400BadRequest, cancellation: cancellationToken);
                break;
        }
    }
}
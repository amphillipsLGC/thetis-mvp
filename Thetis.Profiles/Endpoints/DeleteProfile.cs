using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Thetis.Common.Exceptions;
using Thetis.Profiles.Application.Services;

namespace Thetis.Profiles.Endpoints;

internal class DeleteProfile(IProfileService profileService) : Endpoint<string, Results<NoContent, ProblemDetails>>
{
    public override void Configure()
    {
        Delete("/profiles/{id}");
        Description(x => x
            .WithName("Delete a profile")
            .Produces<NoContent>(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500));
        AllowAnonymous();
    }

    public override async Task HandleAsync(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var profileId))
        {
            await SendAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid profile ID format.",
                TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
            }, StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }

        var result = await profileService.DeleteProfileAsync(profileId, cancellationToken);

        var response = result.Match<IResult>(
            success => TypedResults.NoContent(),
            error => error switch
            {
                EntityNotFoundException _ => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Profile with ID {profileId} not found.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                },
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = $"An unexpected error occurred while deleting the profile. See trace ID: {Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier} for more details.",
                    TraceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier,
                }
            }
        );
    }
}
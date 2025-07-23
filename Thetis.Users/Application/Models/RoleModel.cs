using Thetis.Users.Domain;

namespace Thetis.Users.Application.Models;

internal record RoleModel
(
    Guid Id,
    string Name,
    string Description,
    List<RoleClaimModel>? Claims
);

internal record RoleClaimModel(Guid Id, string ClaimType, string ClaimValue);

internal static class RoleExtensions
{
    public static RoleModel ToModel(this Domain.Role role)
    {
        return new RoleModel(
            role.Id,
            role.Name,
            role.Description,
            role.Claims.Select(c => new RoleClaimModel(c.Id, c.ClaimType, c.ClaimValue)).ToList()
        );
    }

    public static Role ToEntity(this RoleModel model)
    {
        return new Role
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Claims = model.Claims?.Select(c => new Domain.RoleClaim
            {
                Id = c.Id,
                ClaimType = c.ClaimType,
                ClaimValue = c.ClaimValue
            }).ToList() ?? []
        };
    }
}
using Thetis.Profiles.Domain;

namespace Thetis.Profiles.Application.Models;

internal record ProfileModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
    public ProfileOwnerModel Owner { get; set; } = null!;
    public List<DataRequirementModel> DataRequirements { get; set; } = [];
}

internal record ProfileOwnerModel
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

internal record DataRequirementModel
{
    public Guid Id { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public List<DataRequirementRuleModel> Rules { get; set; } = [];
}

internal record DataRequirementRuleModel
{
    public string PropertyName { get; set; } = string.Empty;
    public RuleOperator Operator { get; set; }
    public string? OperatorValue { get; set; }
}

internal static class ProfileExtensions
{
    public static ProfileModel ToModel(this Profile profile)
    {
        return new ProfileModel
        {
            Id = profile.Id,
            Name = profile.Name,
            Description = profile.Description,
            IsPublic = profile.IsPublic,
            Owner = new ProfileOwnerModel
            {
                UserId = profile.Owner.UserId,
                FirstName = profile.Owner.FirstName,
                LastName = profile.Owner.LastName
            },
            DataRequirements = profile.DataRequirements.Select(dr => new DataRequirementModel
            {
                Id = dr.Id,
                ResourceType = dr.ResourceType,
                Rules = dr.Rules.Select(r => new DataRequirementRuleModel
                {
                    PropertyName = r.PropertyName,
                    Operator = r.Operator,
                    OperatorValue = r.OperatorValue
                }).ToList()
            }).ToList()
        };
    }
    
    public static Profile ToEntity(this ProfileModel model)
    {
        return new Profile
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            IsPublic = model.IsPublic,
            Owner = new ProfileOwner
            {
                UserId = model.Owner.UserId,
                FirstName = model.Owner.FirstName,
                LastName = model.Owner.LastName
            },
            DataRequirements = model.DataRequirements.Select(dr => new DataRequirement
            {
                Id = dr.Id,
                ResourceType = dr.ResourceType,
                Rules = dr.Rules.Select(r => new DataRequirementRule
                {
                    PropertyName = r.PropertyName,
                    Operator = r.Operator,
                    OperatorValue = r.OperatorValue
                }).ToList()
            }).ToList()
        };
    }
}
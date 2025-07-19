namespace Thetis.Profiles.Domain;

internal class Profile
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    
    public virtual required ProfileOwner Owner { get; set; }
    public virtual List<DataRequirement> DataRequirements { get; set; } = [];
}

internal class ProfileOwner
{
    public required Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public virtual List<Profile> Profiles { get; set; } = [];
}

internal class DataRequirement
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ProfileId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public List<DataRequirementRule> Rules { get; set; } = [];
    
    public virtual Profile? Profile { get; set; }
}

internal class DataRequirementRule
{
    public string PropertyName { get; set; } = string.Empty;
    public RuleOperator Operator { get; set; }
    public string? OperatorValue { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Thetis.Users.Domain;

internal class User
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Username { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public bool EmailVerified { get; set; }
    public string? PasswordHash { get; set; }
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedOn { get; set; }
    public DateTimeOffset? LastLogin { get; set; }
    public bool IsDeleted { get; set; }
    public List<Role> Roles { get; set; } = [];
}

internal class Role
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<User> Users { get; set; } = [];
    public List<RoleClaim> Claims { get; set; } = [];
}

internal class RoleClaim
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid RoleId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    
    public virtual Role Role { get; set; } = null!;

    public virtual Claim ToClaim()
    {
        return new Claim(ClaimType, ClaimValue);
    }
    
    public virtual void FromClaim(Claim claim)
    {
        ClaimType = claim.Type;
        ClaimValue = claim.Value;
    }
}
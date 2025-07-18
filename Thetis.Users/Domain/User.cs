using System.Security.Claims;

namespace Thetis.Users.Domain;

internal class User
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; } = false;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedOn { get; set; } = null;
    public DateTimeOffset? LastLogin { get; set; } = null;

    public virtual ICollection<UserRole> Roles { get; set; } = [];
}

internal class Role
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public virtual ICollection<UserRole> Users { get; set; } = [];
    public virtual ICollection<RoleClaim> Claims { get; set; } = [];
}

internal class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}

internal class RoleClaim
{
    public Guid UserId { get; set; }
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
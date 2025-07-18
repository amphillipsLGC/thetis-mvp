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
}

internal class Role
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Claim> Claims { get; set; } = [];
}

internal class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}

internal class Claim
{
    public Guid UserId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}
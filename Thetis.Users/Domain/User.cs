namespace Thetis.Users.Domain;

public class User
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedOn { get; set; } = null;
    public DateTimeOffset? LastLogin { get; set; } = null;
}
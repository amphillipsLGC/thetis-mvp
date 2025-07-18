using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thetis.Users.Domain;

namespace Thetis.Users.Data;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DefaultNameLength);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DefaultNameLength);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DefaultEmailLength);

        builder.HasIndex(u => u.Email)
            .IsUnique();
        
        builder.HasData(GetDefaultUsers());

        builder.HasMany(e => e.Roles)
            .WithMany(e => e.Users);

    }
    
    private IEnumerable<User> GetDefaultUsers()
    {
        // Password is Admin123!
        yield return new User
        {
            Id = Guid.CreateVersion7(),
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            EmailVerified = true,
            PasswordHash = "gw5du/XgGNMDFbSNI/XBaA==:0tiw1hzqokOFaWVslwT+rM0eFUeLE71nSUmKYkzAE9s="
        };
    }
}
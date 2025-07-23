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

        builder.Property(u => u.Username)
            .HasMaxLength(DataSchemaConstants.DefaultNameLength);
        
        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(512);

        builder.HasMany(u => u.Roles)
            .WithOne(r =>   r.User)
            .HasForeignKey(k => k.UserId);
    }
}

internal class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UserId);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(ur => ur.RoleId);
    }
}
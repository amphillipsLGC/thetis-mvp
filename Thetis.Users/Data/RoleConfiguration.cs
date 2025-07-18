using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thetis.Users.Domain;

namespace Thetis.Users.Data;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DefaultNameLength);

        builder.Property(r => r.Description)
            .HasMaxLength(DataSchemaConstants.DefaultDescriptionLength);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.HasMany(r => r.Users)
            .WithOne(u => u.Role)
            .HasForeignKey(k => k.RoleId);
            
        
        builder.HasMany(r => r.Claims)
            .WithOne(c => c.Role)
            .HasForeignKey(k => k.RoleId);
    }
}
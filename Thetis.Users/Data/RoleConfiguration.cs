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
    }
}
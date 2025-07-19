using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thetis.Profiles.Domain;

namespace Thetis.Profiles.Data;

internal partial class ProfileOwnerConfiguration : IEntityTypeConfiguration<ProfileOwner>
{
    public void Configure(EntityTypeBuilder<ProfileOwner> builder)
    {
        builder.ToTable("ProfileOwners");

        builder.HasKey(po => po.UserId);

        builder.Property(po => po.FirstName)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DefaultNameLength);

        builder.Property(po => po.LastName)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DefaultNameLength);
        
    }
}
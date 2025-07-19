using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thetis.Profiles.Domain;

namespace Thetis.Profiles.Data;

internal class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");

        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DefaultNameLength);

        builder.Property(p => p.Description)
            .HasMaxLength(DataSchemaConstants.DefaultDescriptionLength);
        
        builder.HasOne(p => p.Owner)
            .WithMany(po => po.Profiles)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(p => p.DataRequirements)
            .WithOne(dr => dr.Profile)
            .HasForeignKey(dr => dr.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.Name);
    }
}
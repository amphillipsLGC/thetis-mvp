using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thetis.Profiles.Domain;

namespace Thetis.Profiles.Data;

internal partial class ProfileOwnerConfiguration
{
    internal class DataRequirementConfiguration : IEntityTypeConfiguration<DataRequirement>
    {
        public void Configure(EntityTypeBuilder<DataRequirement> builder)
        {
            builder.ToTable("DataRequirements");
            
            builder.HasKey(dr => dr.Id);
            
            builder.Property(dr => dr.ResourceType)
                .IsRequired()
                .HasMaxLength(DataSchemaConstants.DefaultNameLength);
            
            builder.Property(dr => dr.Rules)
                .HasColumnType("jsonb");
        }
    }
}
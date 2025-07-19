using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Thetis.Profiles.Domain;

namespace Thetis.Profiles.Data;

internal class ProfileDbContext(DbContextOptions<ProfileDbContext> options) : DbContext(options)
{
    internal DbSet<Profile> Profiles { get; set; }
    internal DbSet<ProfileOwner> ProfileOwners { get; set; }
    internal DbSet<DataRequirement> DataRequirements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DataSchemaConstants.Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
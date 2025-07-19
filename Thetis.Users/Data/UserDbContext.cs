using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Thetis.Users.Domain;

namespace Thetis.Users.Data;

internal class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    internal DbSet<User> Users { get; set; }
    internal DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DataSchemaConstants.Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
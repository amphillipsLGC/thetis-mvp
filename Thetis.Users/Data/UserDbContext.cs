using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Thetis.Users.Domain;

namespace Thetis.Users.Data;

public class UserDbContext : DbContext
{
    internal DbSet<User> Users { get; set; }
    internal DbSet<Role> Roles { get; set; }
    
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DataSchemaConstants.Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CRM.Data // Replace YourNamespace with your project's namespace
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        // Define your DbSets here
        public DbSet<Users> user { get; set; }
        public DbSet<Customers> customer { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure model relationships, constraints, etc. here if needed
        }
    }
}
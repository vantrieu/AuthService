using AuthService.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserClient>().HasKey(m => new { m.ApplicationUserId, m.ClientId });
            modelBuilder.Entity<UserIdentityResource>().HasKey(m => new { m.ApplicationUserId, m.IdentityResourceId });
            modelBuilder.Entity<UserApiScope>().HasKey(m => new { m.ApplicationUserId, m.ApiScopeId });
            modelBuilder.Entity<UserApiResource>().HasKey(m => new { m.ApplicationUserId, m.ApiResourceId });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<UserClient> UserClients { get; set; }

        public DbSet<UserIdentityResource> UserIdentityResources { get; set; }

        public DbSet<UserApiScope> UserApiScopes { get; set; }

        public DbSet<UserApiResource> UserApiResources { get; set; }
    }
}

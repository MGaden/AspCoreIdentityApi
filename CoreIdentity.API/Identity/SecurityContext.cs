using CoreIdentity.API.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreIdentity.API.Identity
{
    public class SecurityContext : IdentityDbContext<IdentityUser>
    {
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }


        public SecurityContext(DbContextOptions<SecurityContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SecurityContextSeedData.Seed(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }
    }
}

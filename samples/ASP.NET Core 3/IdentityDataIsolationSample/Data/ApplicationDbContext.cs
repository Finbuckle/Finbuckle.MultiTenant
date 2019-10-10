using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace IdentityDataIsolationSample.Data
{
    public class ApplicationDbContext : MultiTenantIdentityDbContext
    {
        public ApplicationDbContext(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        public ApplicationDbContext(TenantInfo tenantInfo, DbContextOptions<ApplicationDbContext> options)
            : base(tenantInfo, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MultiTenantIdentityUser>()
                   .Property(e => e.Id)
                   .ValueGeneratedOnAdd();
            base.OnModelCreating(builder);
        }
    }
}

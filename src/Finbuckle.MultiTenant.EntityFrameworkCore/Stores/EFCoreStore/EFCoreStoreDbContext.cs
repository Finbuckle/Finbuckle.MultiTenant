// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Finbuckle.MultiTenant.Internal;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.Stores
{
    public class EFCoreStoreDbContext<TTenantInfo> : DbContext
        where TTenantInfo : class, ITenantInfo, new()
    {
        public EFCoreStoreDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TTenantInfo> TenantInfo => Set<TTenantInfo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TTenantInfo>().HasKey(ti => ti.Id);
            modelBuilder.Entity<TTenantInfo>().Property(ti => ti.Id).HasMaxLength(Constants.TenantIdMaxLength);
            modelBuilder.Entity<TTenantInfo>().HasIndex(ti => ti.Identifier).IsUnique();
        }
    }
}
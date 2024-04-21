// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;

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
            modelBuilder.Entity<TTenantInfo>().Property(ti => ti.Id).HasMaxLength(Internal.Constants.TenantIdMaxLength);
            modelBuilder.Entity<TTenantInfo>().HasIndex(ti => ti.Identifier).IsUnique();
        }
}
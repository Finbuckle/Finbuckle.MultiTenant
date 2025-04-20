// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;

public class EFCoreStoreDbContext<TTenantInfo> : DbContext, IEFCoreStoreTenants<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    public EFCoreStoreDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<TTenantInfo> TenantInfo => Set<TTenantInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TenantInfoEntityConfiguration<TTenantInfo>());
    }
}

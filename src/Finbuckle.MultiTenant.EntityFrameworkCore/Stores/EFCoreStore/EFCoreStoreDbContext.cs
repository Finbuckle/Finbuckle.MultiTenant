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

    public DbSet<TTenantInfo> TenantInfos => Set<TTenantInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureTenantInfoEntity<TTenantInfo>();
    }
}
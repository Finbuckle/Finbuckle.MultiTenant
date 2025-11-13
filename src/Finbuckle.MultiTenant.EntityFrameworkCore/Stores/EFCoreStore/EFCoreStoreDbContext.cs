// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;

/// <summary>
/// A DbContext specialized for storing tenant information in Entity Framework Core.
/// </summary>
/// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
public class EFCoreStoreDbContext<TTenantInfo> : DbContext
    where TTenantInfo : TenantInfo
{
    /// <summary>
    /// Initializes a new instance of EFCoreStoreDbContext.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public EFCoreStoreDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Gets the DbSet of tenant information.
    /// </summary>
    public DbSet<TTenantInfo> TenantInfo => Set<TTenantInfo>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
            modelBuilder.Entity<TTenantInfo>().HasKey(ti => ti.Id);
            modelBuilder.Entity<TTenantInfo>().Property(ti => ti.Id);
            modelBuilder.Entity<TTenantInfo>().HasIndex(ti => ti.Identifier).IsUnique();
        }
}
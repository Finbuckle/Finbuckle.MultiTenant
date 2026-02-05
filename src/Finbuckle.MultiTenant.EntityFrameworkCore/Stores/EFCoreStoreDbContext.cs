// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores;

/// <summary>
/// A <see cref="DbContext"/> specialized for storing tenant information in Entity Framework Core.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class EFCoreStoreDbContext<TTenantInfo> : DbContext
    where TTenantInfo : class, ITenantInfo
{
    /// <summary>
    /// Initializes a new instance of EFCoreStoreDbContext.
    /// </summary>
    /// <param name="options">The <see cref="DbContextOptions"/> instance.</param>
    public EFCoreStoreDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> of tenant information.
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
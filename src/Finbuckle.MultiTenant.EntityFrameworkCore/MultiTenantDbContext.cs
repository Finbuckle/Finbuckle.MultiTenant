// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// A <see cref="DbContext"/> that enforces tenant integrity on multi-tenant entity types.
/// </summary>
public abstract class MultiTenantDbContext<TId> : DbContext, IMultiTenantDbContext<TId> where TId : IEquatable<TId>
{
    /// <inheritdoc />
    public ITenantInfo<TId>? TenantInfo
    {
        get;
        set
        {
            value?.EnsureValid();
            field = value;
        }
    }

    /// <inheritdoc />
    public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

    /// <inheritdoc />
    public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;


    /// <summary>
    /// Constructs the database context instance.
    /// </summary>
    protected MultiTenantDbContext()
    {
    }

    /// <summary>
    /// Constructs the database context instance with the provided options.
    /// </summary>
    /// <param name="options">The <see cref="DbContextOptions"/> instance.</param>
    protected MultiTenantDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureMultiTenant<TId>();
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.EnforceMultiTenant<MultiTenantDbContext<TId>, TId>();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        this.EnforceMultiTenant<MultiTenantDbContext<TId>, TId>();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
    }
}
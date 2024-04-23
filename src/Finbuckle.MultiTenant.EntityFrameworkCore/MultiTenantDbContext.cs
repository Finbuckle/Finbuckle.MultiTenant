// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// A database context that enforces tenant integrity on multi-tenant entity types.
/// </summary>
public abstract class MultiTenantDbContext : DbContext, IMultiTenantDbContext
{
    /// <inheritdoc />
    public ITenantInfo? TenantInfo { get; internal set; }

    /// <inheritdoc />
    public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

    /// <inheritdoc />
    public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

    protected MultiTenantDbContext(ITenantInfo? tenantInfo)
    {
        TenantInfo = tenantInfo;
    }

    /// <summary>
    /// Constructs the database context instance and binds to the current tenant.
    /// </summary>
    /// <param name="multiTenantContextAccessor">The MultiTenantContextAccessor instance used to bind the context instance to a tenant.</param>
    protected MultiTenantDbContext(IMultiTenantContextAccessor multiTenantContextAccessor)
    {
        TenantInfo = multiTenantContextAccessor.MultiTenantContext.TenantInfo;
    }

    protected MultiTenantDbContext(ITenantInfo? tenantInfo, DbContextOptions options) : base(options)
    {
        TenantInfo = tenantInfo;
    }

    /// <summary>
    /// Constructs the database context instance and binds to the current tenant.
    /// </summary>
    /// <param name="multiTenantContextAccessor">The MultiTenantContextAccessor instance used to bind the context instance to a tenant.</param>
    /// <param name="options">The database options instance.</param>
    protected MultiTenantDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) : base(options)
    {
        TenantInfo = multiTenantContextAccessor.MultiTenantContext.TenantInfo;
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureMultiTenant();
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.EnforceMultiTenant();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        this.EnforceMultiTenant();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;
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

    /// <summary>
    /// Creates a new instance of a multitenant context that accepts a IMultiTenantContextAccessor instance and an optional DbContextOptions instance.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <param name="options">The database options instance.</param>
    /// <typeparam name="TContext">The TContext implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns></returns>
    public static TContext Create<TContext, TTenantInfo>(TTenantInfo? tenantInfo, DbContextOptions? options = null)
        where TContext : DbContext
        where TTenantInfo : class, ITenantInfo, new()
    {
        try
        {
            var mca = new StaticMultiTenantContextAccessor<TTenantInfo>(tenantInfo);
            var context = options switch
            {
                null => (TContext)Activator.CreateInstance(typeof(TContext), mca)!,
                not null => (TContext)Activator.CreateInstance(typeof(TContext), mca, options)!
            };
            
            return context;
        }
        catch (MissingMethodException)
        {
            throw new ArgumentException("The provided DbContext type does not have a constructor that accepts the required parameters.");
        }
    }

    /// <summary>
    /// Constructs the database context instance and binds to the current tenant.
    /// </summary>
    /// <param name="multiTenantContextAccessor">The MultiTenantContextAccessor instance used to bind the context instance to a tenant.</param>
    protected MultiTenantDbContext(IMultiTenantContextAccessor multiTenantContextAccessor)
    {
        TenantInfo = multiTenantContextAccessor.MultiTenantContext.TenantInfo;
    }

    /// <summary>
    /// Constructs the database context instance and binds to the current tenant.
    /// </summary>
    /// <param name="multiTenantContextAccessor">The MultiTenantContextAccessor instance used to bind the context instance to a tenant.</param>
    /// <param name="options">The database options instance.</param>
    protected MultiTenantDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) :
        base(options)
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
// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
    /// Creates a new instance of a DbContext that accepts an ITenantInfo instance.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <typeparam name="TContext">The TContext implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The newly created DbContext instance.</returns>
    public static TContext Create<TContext, TTenantInfo>(TTenantInfo tenantInfo)
        where TContext : DbContext
        where TTenantInfo : class, ITenantInfo, new()
    => Create<TContext, TTenantInfo>(tenantInfo, []);
    
    /// <summary>
    /// Creates a new instance of a DbContext that accepts an ITenantInfo instance and optional dependencies.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <param name="args">Additional dependencies for the DbContext constructor.</param>
    /// <typeparam name="TContext">The TContext implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The newly created DbContext instance.</returns>
    public static TContext Create<TContext, TTenantInfo>(TTenantInfo tenantInfo, params object[] args)
        where TContext : DbContext
        where TTenantInfo : class, ITenantInfo, new()
    {
        try
        {
            var mca = new StaticMultiTenantContextAccessor<TTenantInfo>(tenantInfo);

            args ??= [];
            object?[] argsList = [mca, ..args];
            
            var context = (TContext)Activator.CreateInstance(typeof(TContext), argsList)!;
            return context;
        }
        catch (MissingMethodException e)
        {
            throw new ArgumentException(
                "The provided DbContext type does not have a constructor that accepts the required parameters.", e);
        }
    }

    /// <summary>
    /// Creates a new instance of a DbContext that accepts an ITenantInfo instance, a service provider, and optional dependencies.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <param name="serviceProvider">The IServiceProvider used to resolve DbContext constructor dependencies.</param>
    /// <param name="args">Additional dependencies for the DbContext constructor.</param>
    /// <typeparam name="TContext">The TContext implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The newly created DbContext instance.</returns>
    public static TContext Create<TContext, TTenantInfo>(TTenantInfo tenantInfo, IServiceProvider serviceProvider, params object[] args)
        where TContext : DbContext
        where TTenantInfo : class, ITenantInfo, new()
    {
        try
        {
            var mca = new StaticMultiTenantContextAccessor<TTenantInfo>(tenantInfo);

            args ??= [];
            object[] argsList = [mca, ..args];
            
            var context = ActivatorUtilities.CreateInstance<TContext>(serviceProvider, argsList)!;
            return context;
        }
        catch (MissingMethodException e)
        {
            throw new ArgumentException(
                "The provided DbContext type does not have a constructor that accepts the required parameters.", e);
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
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
    }
}
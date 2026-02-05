// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// A <see cref="DbContext"/> that enforces tenant integrity on multi-tenant entity types.
/// </summary>
public abstract class MultiTenantDbContext : DbContext, IMultiTenantDbContext
{
    #region Fork Sirfull : set public
    /// <inheritdoc />
    // internal set for testing
    //public ITenantInfo? TenantInfo { get; internal set; }
    public ITenantInfo? TenantInfo { get; set; }
    #endregion

    /// <inheritdoc />
    public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

    /// <inheritdoc />
    public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

    /// <inheritdoc />
    public bool IsMultiTenantEnabled { get; set; } = true;

    /// <summary>
    /// Creates a new instance of a <see cref="DbContext"/> that accepts an <see cref="TenantInfo"/> instance.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <typeparam name="TContext">The <see cref="DbContext"/> implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <returns>The newly created <see cref="DbContext"/> instance.</returns>
    public static TContext Create<TContext, TTenantInfo>(TTenantInfo tenantInfo)
        where TContext : DbContext
        where TTenantInfo : ITenantInfo
        => Create<TContext, TTenantInfo>(tenantInfo, []);

    /// <summary>
    /// Creates a new instance of a <see cref="DbContext"/> that accepts an <see cref="TenantInfo"/> instance and optional dependencies.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <param name="args">Additional dependencies for the <see cref="DbContext"/> constructor.</param>
    /// <typeparam name="TContext">The <see cref="DbContext"/> implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <returns>The newly created <see cref="DbContext"/> instance.</returns>
    public static TContext Create<TContext, TTenantInfo>(TTenantInfo tenantInfo, params object[] args)
        where TContext : DbContext
        where TTenantInfo : ITenantInfo
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
    /// Creates a new instance of a <see cref="DbContext"/> that accepts an <see cref="TenantInfo"/> instance, a service provider, and optional dependencies.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to resolve <see cref="DbContext"/> constructor dependencies.</param>
    /// <param name="args">Additional dependencies for the <see cref="DbContext"/> constructor.</param>
    /// <typeparam name="TContext">The <see cref="DbContext"/> implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <returns>The newly created <see cref="DbContext"/> instance.</returns>
    public static TContext Create<TContext, TTenantInfo>(TTenantInfo tenantInfo, IServiceProvider serviceProvider,
        params object[] args)
        where TContext : DbContext
        where TTenantInfo : ITenantInfo
    {
        try
        {
            var mca = new StaticMultiTenantContextAccessor<TTenantInfo>(tenantInfo);

            args ??= [];
            object[] argsList = [mca, ..args];

            var context = ActivatorUtilities.CreateInstance<TContext>(serviceProvider, argsList);
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
    /// <param name="multiTenantContextAccessor">The <see cref="IMultiTenantContextAccessor"/> instance used to bind the context instance to a tenant.</param>
    protected MultiTenantDbContext(IMultiTenantContextAccessor multiTenantContextAccessor)
    {
        TenantInfo = multiTenantContextAccessor.MultiTenantContext.TenantInfo;
    }

    /// <summary>
    /// Constructs the database context instance and binds to the current tenant.
    /// </summary>
    /// <param name="multiTenantContextAccessor">The <see cref="IMultiTenantContextAccessor"/> instance used to bind the context instance to a tenant.</param>
    /// <param name="options">The <see cref="DbContextOptions"/> instance.</param>
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
        if (IsMultiTenantEnabled)
        {
            this.EnforceMultiTenant();
        }
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (IsMultiTenantEnabled)
        {
            this.EnforceMultiTenant();
        }
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
    }
}

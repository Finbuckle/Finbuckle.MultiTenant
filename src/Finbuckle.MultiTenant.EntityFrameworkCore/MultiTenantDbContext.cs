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
    /// <inheritdoc />
    public ITenantInfo? TenantInfo { get; set; }

    /// <inheritdoc />
    public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

    /// <inheritdoc />
    public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

    /// <summary>
    /// Creates a new instance of a <see cref="DbContext"/> bound to the given tenant.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <typeparam name="TContext">The <see cref="DbContext"/> implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <returns>The newly created <see cref="DbContext"/> instance.</returns>
    public static TContext Create<TContext, TTenantInfo>(TTenantInfo tenantInfo)
        where TContext : DbContext, IMultiTenantDbContext
        where TTenantInfo : ITenantInfo
        => Create<TContext, TTenantInfo>(tenantInfo, []);

    /// <summary>
    /// Creates a new instance of a <see cref="DbContext"/> bound to the given tenant, with optional constructor dependencies.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <param name="args">Additional dependencies for the <see cref="DbContext"/> constructor.</param>
    /// <typeparam name="TContext">The <see cref="DbContext"/> implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <returns>The newly created <see cref="DbContext"/> instance.</returns>
    public static TContext Create<TContext, TTenantInfo>(TTenantInfo tenantInfo, params object[] args)
        where TContext : DbContext, IMultiTenantDbContext
        where TTenantInfo : ITenantInfo
    {
        try
        {
            args ??= [];
            var context = (TContext)Activator.CreateInstance(typeof(TContext), args)!;
            context.TenantInfo = tenantInfo;
            return context;
        }
        catch (MissingMethodException e)
        {
            throw new ArgumentException(
                "The provided DbContext type does not have a constructor that accepts the required parameters.", e);
        }
    }

    /// <summary>
    /// Creates a new instance of a <see cref="DbContext"/> bound to the given tenant, resolving dependencies from the provided service provider.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to resolve <see cref="DbContext"/> constructor dependencies.</param>
    /// <param name="args">Additional dependencies for the <see cref="DbContext"/> constructor.</param>
    /// <typeparam name="TContext">The <see cref="DbContext"/> implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <returns>The newly created <see cref="DbContext"/> instance.</returns>
    public static TContext Create<TContext, TTenantInfo>(TTenantInfo tenantInfo, IServiceProvider serviceProvider,
        params object[] args)
        where TContext : DbContext, IMultiTenantDbContext
        where TTenantInfo : ITenantInfo
    {
        try
        {
            args ??= [];
            var context = ActivatorUtilities.CreateInstance<TContext>(serviceProvider, args);
            context.TenantInfo = tenantInfo;
            return context;
        }
        catch (MissingMethodException e)
        {
            throw new ArgumentException(
                "The provided DbContext type does not have a constructor that accepts the required parameters.", e);
        }
    }

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
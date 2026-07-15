// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> extension methods for Finbuckle.MultiTenant.EntityFrameworkCore.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a pooled factory for a MultiTenant db context.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="optionsAction">An action to configure the <see cref="DbContextOptionsBuilder"/> for the pooled factory.</param>
    /// <param name="poolSize">The maximum number of <typeparamref name="T"/> instances retained by the pool. Defaults to 1024.</param>
    /// <typeparam name="T">
    /// The <see cref="DbContext"/> type to resolve from the pool. Must implement
    /// <see cref="IMultiTenantDbContext"/> so tenant context can be applied.
    /// </typeparam>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddPooledMultiTenantDbContext<T>(this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction,
        int poolSize = 1024)
        where T : DbContext, IMultiTenantDbContext
    {
        services.AddPooledDbContextFactory<T>(optionsAction, poolSize);
        services.AddScoped<T>(sp =>
        {
            var pooledFactory = sp.GetRequiredService<IDbContextFactory<T>>();
            var context = pooledFactory.CreateDbContext();
            context.ChangeTracker.Clear();

            var tenantInfo = sp.GetRequiredService<ITenantContext>().TenantInfo;
            context.TenantInfo = tenantInfo;
            context.EnforceMultiTenantOnTracking();

            return context;
        });

        return services;
    }

    /// <summary>
    /// Registers a MultiTenant db context as a scoped service.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="DbContext"/> type to register. Must implement
    /// <see cref="IMultiTenantDbContext"/> so tenant context can be applied.
    /// </typeparam>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddMultiTenantDbContext<T>(this IServiceCollection services)
        where T : DbContext, IMultiTenantDbContext
        => services.AddMultiTenantDbContext<T>((_, _) => { });

    /// <summary>
    /// Registers a MultiTenant db context as a scoped service.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="optionsAction">An action to configure the <see cref="DbContextOptionsBuilder"/>.</param>
    /// <typeparam name="T">
    /// The <see cref="DbContext"/> type to register. Must implement
    /// <see cref="IMultiTenantDbContext"/> so tenant context can be applied.
    /// </typeparam>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddMultiTenantDbContext<T>(this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
        where T : DbContext, IMultiTenantDbContext
        => services.AddMultiTenantDbContext<T>((_, b) => optionsAction(b));

    /// <summary>
    /// Registers a MultiTenant db context as a scoped service.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="optionsAction">An action to configure the <see cref="DbContextOptionsBuilder"/> with access to the <see cref="IServiceProvider"/>.</param>
    /// <typeparam name="T">
    /// The <see cref="DbContext"/> type to register. Must implement
    /// <see cref="IMultiTenantDbContext"/> so tenant context can be applied.
    /// </typeparam>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddMultiTenantDbContext<T>(this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
        where T : DbContext, IMultiTenantDbContext
    {
        services.AddDbContextFactory<T>(optionsAction);
        services.AddScoped<T>(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<T>>();
            var context = factory.CreateDbContext();
            var tenantInfo = sp.GetRequiredService<ITenantContext>().TenantInfo;
            context.TenantInfo = tenantInfo;
            context.EnforceMultiTenantOnTracking();

            return context;
        });

        return services;
    }
}

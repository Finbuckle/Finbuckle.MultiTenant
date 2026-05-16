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
            var tenantInfo = sp.GetRequiredService<IMultiTenantContextAccessor>().MultiTenantContext.TenantInfo;
            context.TenantInfo = tenantInfo;
            context.EnforceMultiTenantOnTracking();

            return context;
        });

        return services;
    }
}
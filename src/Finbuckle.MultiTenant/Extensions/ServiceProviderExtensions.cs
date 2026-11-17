// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.Extensions;

/// <summary>
/// <see cref="IServiceProvider"/> extension methods for Finbuckle.MultiTenant.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Begins a new ambient tenant scope for the current asynchronous execution context.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> instance the extension method applies to.</param>
    public static void BeginTenantScope(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.GetRequiredService<ITenantScopeProvider>().BeginScope();
    }

    /// <summary>
    /// Begins a new ambient tenant scope for the current asynchronous execution context and sets its tenant information.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> instance the extension method applies to.</param>
    /// <param name="tenantInfo">The tenant information for the new scope.</param>
    public static void BeginTenantScope<TId>(this IServiceProvider services, ITenantInfo<TId> tenantInfo) where TId : IEquatable<TId>
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(tenantInfo);

        var tenantScopeProvider = services.GetRequiredService<ITenantScopeProvider>();
        tenantScopeProvider.BeginScope();
        var tenantContext = services.GetRequiredService<ITenantContext<TId>>();
        tenantContext.TenantInfo = tenantInfo;
    }

    /// <summary>
    /// Runs a delegate as the given tenant in a new dependency injection scope and a new ambient tenant scope.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> instance the extension method applies to.</param>
    /// <param name="tenantInfo">The tenant information for the new scope.</param>
    /// <param name="action">
    /// The delegate to run. It receives the new scope's <see cref="IServiceProvider"/>; resolve services from it so
    /// scoped services such as a DbContext are created for <paramref name="tenantInfo"/>.
    /// </param>
    /// <typeparam name="TId">The ID implementation type.</typeparam>
    /// <returns>A task that completes when <paramref name="action"/> completes and the scope is disposed.</returns>
    /// <remarks>
    /// The new dependency injection scope is disposed when <paramref name="action"/> completes, even if it throws.
    /// The caller's ambient tenant scope, or the absence of one, is unchanged when this method returns, so it is
    /// safe to call during an HTTP request or inside another tenant scope.
    /// </remarks>
    public static async Task RunInTenantScopeAsync<TId>(this IServiceProvider services, ITenantInfo<TId> tenantInfo,
        Func<IServiceProvider, Task> action) where TId : IEquatable<TId>
    {
        ArgumentNullException.ThrowIfNull(action);

        await services.RunInTenantScopeAsync(tenantInfo, async scopedServices =>
        {
            await action(scopedServices).ConfigureAwait(false);
            return true;
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs a delegate as the given tenant in a new dependency injection scope and a new ambient tenant scope, and
    /// returns its result.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> instance the extension method applies to.</param>
    /// <param name="tenantInfo">The tenant information for the new scope.</param>
    /// <param name="action">
    /// The delegate to run. It receives the new scope's <see cref="IServiceProvider"/>; resolve services from it so
    /// scoped services such as a DbContext are created for <paramref name="tenantInfo"/>.
    /// </param>
    /// <typeparam name="TId">The ID implementation type.</typeparam>
    /// <typeparam name="TResult">The type of the delegate's result.</typeparam>
    /// <returns>A task that completes with the result of <paramref name="action"/> after the scope is disposed.</returns>
    /// <remarks>
    /// The new dependency injection scope is disposed when <paramref name="action"/> completes, even if it throws.
    /// The caller's ambient tenant scope, or the absence of one, is unchanged when this method returns, so it is
    /// safe to call during an HTTP request or inside another tenant scope.
    /// </remarks>
    public static async Task<TResult> RunInTenantScopeAsync<TId, TResult>(this IServiceProvider services,
        ITenantInfo<TId> tenantInfo, Func<IServiceProvider, Task<TResult>> action) where TId : IEquatable<TId>
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(tenantInfo);
        ArgumentNullException.ThrowIfNull(action);

        // Changes to the ambient tenant scope made inside this async method are reverted for the caller when it
        // returns, so no EndScope call is needed (and one would clear the caller's own scope).
        await using var scope = services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
        scope.ServiceProvider.BeginTenantScope(tenantInfo);
        return await action(scope.ServiceProvider).ConfigureAwait(false);
    }
}

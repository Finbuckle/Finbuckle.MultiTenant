// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Provides builder methods for Finbuckle.MultiTenant services and configuration.
/// </summary>
public static class MultiTenantBuilderExtensions
{
    /// <summary>
    /// Adds an <see cref="EFCoreStore{TEFCoreStoreDbContext,TTenantInfo, TId}"/> based multi-tenant store to the application. Will also add the database context service unless it is already added.
    /// </summary>
    /// <typeparam name="TEFCoreStoreDbContext">The <see cref="EFCoreStoreDbContext{TTenantInfo, TId}"/> derived type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
    /// <typeparam name="TId">The ID implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo, TId}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo, TId}"/> instance.</returns>
    // ReSharper disable once InconsistentNaming
    public static MultiTenantBuilder<TTenantInfo, TId> WithEFCoreStore<TEFCoreStoreDbContext, TTenantInfo, TId>(
        this MultiTenantBuilder<TTenantInfo, TId> builder)
        where TEFCoreStoreDbContext : EFCoreStoreDbContext<TTenantInfo, TId>
        where TTenantInfo : class, ITenantInfo<TId>
        where TId : IEquatable<TId>
    {
        builder.Services
            .AddDbContext<TEFCoreStoreDbContext>(); // Note, will not override existing context if already added.
        return builder.WithStore<EFCoreStore<TEFCoreStoreDbContext, TTenantInfo, TId>>(ServiceLifetime.Scoped);
    }
}
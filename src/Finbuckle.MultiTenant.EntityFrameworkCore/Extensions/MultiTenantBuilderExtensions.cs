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
    /// Adds an <see cref="EFCoreStore{TEFCoreStoreDbContext,TTenantInfo}"/> based multi-tenant store to the application. Will also add the database context service unless it is already added.
    /// </summary>
    /// <typeparam name="TEFCoreStoreDbContext">The <see cref="EFCoreStoreDbContext{TTenantInfo}"/> derived type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</returns>
    // ReSharper disable once InconsistentNaming
    public static MultiTenantBuilder<TTenantInfo> WithEFCoreStore<TEFCoreStoreDbContext, TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TEFCoreStoreDbContext : EFCoreStoreDbContext<TTenantInfo>
        where TTenantInfo : class, ITenantInfo
    {
        builder.Services
            .AddDbContext<TEFCoreStoreDbContext>(); // Note, will not override existing context if already added.
        return builder.WithStore<EFCoreStore<TEFCoreStoreDbContext, TTenantInfo>>(ServiceLifetime.Scoped);
    }
}
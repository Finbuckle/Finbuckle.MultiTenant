// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Provides builder methods for Finbuckle.MultiTenant services and configuration.
/// </summary>
public static class MultiTenantBuilderExtensions
{
    /// <summary>
    /// Adds an EFCore based multi-tenant store to the application. Will also add the database context service unless it is already added.
    /// </summary>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    // ReSharper disable once InconsistentNaming
    public static MultiTenantBuilder<TTenantInfo> WithEFCoreStore<TEFCoreStoreDbContext, TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder)
        where TEFCoreStoreDbContext : EFCoreStoreDbContext<TTenantInfo>
        where TTenantInfo : TenantInfo
    {
        builder.Services.AddDbContext<TEFCoreStoreDbContext>(); // Note, will not override existing context if already added.
        return builder.WithStore<EFCoreStore<TEFCoreStoreDbContext, TTenantInfo>>(ServiceLifetime.Scoped);
    }
}
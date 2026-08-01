// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

internal class TenantStoreLookupInfo<TTenantInfo, TId>
    where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    public IMultiTenantStore<TTenantInfo, TId>? Store { get; init; }

    public IMultiTenantStoreCache<TTenantInfo, TId>? Cache { get; init; }

    public required string Identifier { get; init; }

    public TTenantInfo? TenantInfo { get; init; }
}

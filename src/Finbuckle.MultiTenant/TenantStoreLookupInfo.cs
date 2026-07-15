// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

internal class TenantStoreLookupInfo<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    public IMultiTenantStore<TTenantInfo>? Store { get; init; }

    public IMultiTenantStoreCache<TTenantInfo>? Cache { get; init; }

    public required string Identifier { get; init; }

    public TTenantInfo? TenantInfo { get; init; }
}

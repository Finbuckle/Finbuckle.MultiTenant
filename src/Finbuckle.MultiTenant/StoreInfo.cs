// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;

namespace Finbuckle.MultiTenant
{
    public class StoreInfo<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
    {
        public Type? StoreType { get; internal set; }
        public IMultiTenantStore<TTenantInfo>? Store { get; internal set; }
    }
}
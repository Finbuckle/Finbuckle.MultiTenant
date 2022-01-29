// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;

namespace Finbuckle.MultiTenant
{
    public class TenantResolvedContext
    {
        public object? Context { get; set; }
        public ITenantInfo? TenantInfo { get; set; }
        public Type? StrategyType { get; set; }
        public Type? StoreType { get; set; }
        // TODO consider refactoring to just MultiTenantContext<T>
    }
}
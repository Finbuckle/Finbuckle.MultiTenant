// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace Finbuckle.MultiTenant
{
    public class TenantResolvedContext
    {
        public object? Context { get; set; }
        public ITenantInfo? TenantInfo { get; set; }
    }
}
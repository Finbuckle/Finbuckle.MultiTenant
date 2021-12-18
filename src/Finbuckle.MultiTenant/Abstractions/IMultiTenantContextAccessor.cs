// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace Finbuckle.MultiTenant
{
    public interface IMultiTenantContextAccessor
    {
        object? MultiTenantContext { get; set; }
    }

    public interface IMultiTenantContextAccessor<T> where T : class, ITenantInfo, new()
    {
        IMultiTenantContext<T>? MultiTenantContext { get; set; }
    }
}
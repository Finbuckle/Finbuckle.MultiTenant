// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant
{
    public interface ITenantResolver
    {
        Task<object?> ResolveAsync(object context);
    }

    public interface ITenantResolver<T>
        where T : class, ITenantInfo, new()
    {
        // TODO: Look into this - I think it will never be null.
        IEnumerable<IMultiTenantStrategy> Strategies { get; }
        // TODO: Look into this - I think it will never be null.
        IEnumerable<IMultiTenantStore<T>> Stores { get; }
        
        Task<IMultiTenantContext<T>?> ResolveAsync(object context);
    }
}
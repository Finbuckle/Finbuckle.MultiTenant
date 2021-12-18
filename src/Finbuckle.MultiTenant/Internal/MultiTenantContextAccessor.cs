// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Threading;

namespace Finbuckle.MultiTenant.Core
{
    public class MultiTenantContextAccessor<T> : IMultiTenantContextAccessor<T>, IMultiTenantContextAccessor
        where T : class, ITenantInfo, new()
    {
        internal static AsyncLocal<IMultiTenantContext<T>?> _asyncLocalContext = new AsyncLocal<IMultiTenantContext<T>?>();

        public IMultiTenantContext<T>? MultiTenantContext
        {
            get
            {
                return _asyncLocalContext.Value;
            }

            set
            {
                _asyncLocalContext.Value = value;
            }
        }

        object? IMultiTenantContextAccessor.MultiTenantContext
        {
            get => MultiTenantContext;
            set => MultiTenantContext = value as IMultiTenantContext<T> ?? MultiTenantContext;
        }
    }
}
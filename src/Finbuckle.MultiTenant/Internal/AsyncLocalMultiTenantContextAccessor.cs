// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Threading;

namespace Finbuckle.MultiTenant.Core
{
    public class AsyncLocalMultiTenantContextAccessor<T> : IMultiTenantContextAccessor<T>, IMultiTenantContextAccessor
        where T : class, ITenantInfo, new()
    {
        private static readonly AsyncLocal<IMultiTenantContext<T>?> _asyncLocalContext = new();

        public IMultiTenantContext<T>? MultiTenantContext
        {
            get => _asyncLocalContext.Value;

            set => _asyncLocalContext.Value = value;
        }

        IMultiTenantContext? IMultiTenantContextAccessor.MultiTenantContext
        {
            get => MultiTenantContext as IMultiTenantContext;
            set => MultiTenantContext = value as IMultiTenantContext<T> ?? MultiTenantContext;
        }
    }
}
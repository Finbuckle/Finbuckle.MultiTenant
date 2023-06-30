// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Threading;

namespace Finbuckle.MultiTenant.Internal;

/// <summary>
/// Provides access the current MultiTenantContext via an AsyncLocal variable.
/// </summary>
/// <typeparam name="T">The ITenantInfo implementation type.</typeparam>
/// <remarks>
/// This implementation may have performance impacts due to the use of AsyncLocal.
/// </remarks>
public class AsyncLocalMultiTenantContextAccessor<T> : IMultiTenantContextAccessor<T>, IMultiTenantContextAccessor
    where T : class, ITenantInfo, new()
{
    private static readonly AsyncLocal<IMultiTenantContext<T>?> AsyncLocalContext = new();

    /// <inheritdoc />
    public IMultiTenantContext<T>? MultiTenantContext
    {
        get => AsyncLocalContext.Value;

        set => AsyncLocalContext.Value = value;
    }

    /// <inheritdoc />
    /// TODO move this to the interface?
    IMultiTenantContext? IMultiTenantContextAccessor.MultiTenantContext
    {
        get => MultiTenantContext as IMultiTenantContext;
        set => MultiTenantContext = value as IMultiTenantContext<T> ?? MultiTenantContext;
    }
}
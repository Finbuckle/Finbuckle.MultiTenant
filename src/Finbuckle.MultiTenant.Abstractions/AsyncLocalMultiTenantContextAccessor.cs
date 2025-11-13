// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Provides access to the current <see cref="IMultiTenantContext{TTenantInfo}"/> via an <see cref="AsyncLocal{T}"/> variable.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="TenantInfo"/> derived type.</typeparam>
public class AsyncLocalMultiTenantContextAccessor<TTenantInfo> : IMultiTenantContextSetter,
    IMultiTenantContextAccessor<TTenantInfo>
    where TTenantInfo : TenantInfo
{
    private static readonly AsyncLocal<IMultiTenantContext<TTenantInfo>> AsyncLocalContext = new();

    /// <inheritdoc />
    public IMultiTenantContext<TTenantInfo> MultiTenantContext
    {
        get => AsyncLocalContext.Value ?? (AsyncLocalContext.Value = new MultiTenantContext<TTenantInfo>(null));
        private set => AsyncLocalContext.Value = value;
    }

    /// <inheritdoc />
    IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => MultiTenantContext;

    IMultiTenantContext IMultiTenantContextSetter.MultiTenantContext
    {
        set => MultiTenantContext = (IMultiTenantContext<TTenantInfo>)value;
    }
}
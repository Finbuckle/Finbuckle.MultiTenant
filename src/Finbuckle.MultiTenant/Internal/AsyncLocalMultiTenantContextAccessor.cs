// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Internal;

/// <summary>
/// Provides access the current MultiTenantContext via an AsyncLocal variable.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
internal class AsyncLocalMultiTenantContextAccessor<TTenantInfo> : IMultiTenantContextSetter,
    IMultiTenantContextAccessor<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    private static readonly AsyncLocal<IMultiTenantContext<TTenantInfo>> AsyncLocalContext = new();

    /// <inheritdoc />
    public IMultiTenantContext<TTenantInfo> MultiTenantContext
    {
        get => AsyncLocalContext.Value ?? (AsyncLocalContext.Value = new MultiTenantContext<TTenantInfo>());
        set => AsyncLocalContext.Value = value;
    }

    /// <inheritdoc />
    IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => (IMultiTenantContext)MultiTenantContext;

    IMultiTenantContext IMultiTenantContextSetter.MultiTenantContext
    {
        set => MultiTenantContext = (IMultiTenantContext<TTenantInfo>)value;
    }
}
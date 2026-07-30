// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Provides access to the ambient tenant context for the current asynchronous execution context.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public class AmbientTenantContext<TTenantInfo, TId> : ITenantContext<TTenantInfo, TId>, ITenantScopeProvider
    where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets the ambient tenant context associated with the current asynchronous execution context.
    /// </summary>
    public AsyncLocal<ITenantContext<TTenantInfo, TId>?> TenantContext { get; } = new();

    /// <summary>
    /// Begins a new ambient tenant scope for the current asynchronous execution context.
    /// </summary>
    public void BeginScope()
    {
        TenantContext.Value = new InternalTenantContext<TTenantInfo, TId>();
    }

    ITenantInfo<TId>? ITenantContext<TId>.TenantInfo
    {
        get => TenantInfo;
        set => TenantInfo = (TTenantInfo?)value;
    }

    private ITenantContext<TTenantInfo, TId> GetCurrentContext() =>
        TenantContext.Value ??
        throw new MultiTenantException(
            "No ambient tenant scope has been established.");

    /// <inheritdoc />
    public TTenantInfo? TenantInfo
    {
        get => GetCurrentContext().TenantInfo;
        set => GetCurrentContext().TenantInfo = value;
    }
}

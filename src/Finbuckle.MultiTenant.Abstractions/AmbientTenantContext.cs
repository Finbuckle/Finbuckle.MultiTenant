// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Provides access to the ambient tenant context for the current asynchronous execution context.
/// </summary>
/// <typeparam name="TTenantType">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public class AmbientTenantContext<TTenantType, TId> : ITenantContext<TTenantType, TId>, ITenantScopeProvider
    where TTenantType : ITenantInfo<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets the ambient tenant context associated with the current asynchronous execution context.
    /// </summary>
    public AsyncLocal<ITenantContext<TTenantType, TId>?> TenantContext { get; } = new();

    /// <summary>
    /// Begins a new ambient tenant scope for the current asynchronous execution context.
    /// </summary>
    public void BeginScope()
    {
        TenantContext.Value = new InternalTenantContext<TTenantType, TId>();
    }

    ITenantInfo<TId>? ITenantContext<TId>.TenantInfo
    {
        get => TenantInfo;
        set => TenantInfo = (TTenantType?)value;
    }

    private ITenantContext<TTenantType, TId> GetCurrentContext() =>
        TenantContext.Value ??
        throw new MultiTenantException(
            "No ambient tenant scope has been established.");

    /// <inheritdoc />
    public TTenantType? TenantInfo
    {
        get => GetCurrentContext().TenantInfo;
        set => GetCurrentContext().TenantInfo = value;
    }
}

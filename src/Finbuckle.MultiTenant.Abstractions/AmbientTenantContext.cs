// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Provides access to the ambient tenant context for the current asynchronous execution context.
/// </summary>
/// <typeparam name="TTenantType">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class AmbientTenantContext<TTenantType> : ITenantContext<TTenantType>, ITenantScopeProvider
    where TTenantType : ITenantInfo
{
    /// <summary>
    /// Gets the ambient tenant context associated with the current asynchronous execution context.
    /// </summary>
    public AsyncLocal<ITenantContext<TTenantType>?> TenantContext { get; } = new();

    /// <summary>
    /// Begins a new ambient tenant scope for the current asynchronous execution context.
    /// </summary>
    public void BeginScope()
    {
        TenantContext.Value = new InternalTenantContext<TTenantType>();
    }

    ITenantInfo? ITenantContext.TenantInfo
    {
        get => TenantInfo;
        set => TenantInfo = (TTenantType?)value;
    }

    private ITenantContext<TTenantType> GetCurrentContext() =>
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

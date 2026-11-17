// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Provides operations for managing the ambient tenant scope for the current execution context.
/// </summary>
/// <remarks>
/// Register implementations as a singleton that shares its ambient state with the registered
/// <see cref="ITenantContext{TId}"/>, as the default <see cref="AmbientTenantContext{TTenantInfo, TId}"/> registration
/// does. The ASP.NET Core integration requires this and throws at startup otherwise.
/// </remarks>
public interface ITenantScopeProvider
{
    /// <summary>
    /// Begins a new ambient tenant scope for the current execution context.
    /// </summary>
    public void BeginScope();

    /// <summary>
    /// Ends the ambient tenant scope for the current execution context.
    /// </summary>
    /// <remarks>
    /// After this call there is no ambient tenant scope until <see cref="BeginScope"/> is called again.
    /// Execution contexts that captured the scope before it ended, such as background work started during
    /// the scope, keep the scope they captured.
    /// </remarks>
    public void EndScope();

    /// <summary>
    /// Gets a value indicating whether an ambient tenant scope has been established for the current execution context.
    /// </summary>
    public bool HasScope { get; }
}

// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.AspNetCore.Authentication;

/// <summary>
/// Options for configuring multi-tenant authentication behavior.
/// </summary>
public class MultiTenantAuthenticationOptions
{
    /// <summary>
    /// Gets or sets whether to skip authentication challenges when a tenant is not resolved.
    /// </summary>
    public bool SkipChallengeIfTenantNotResolved { get; set; }
}
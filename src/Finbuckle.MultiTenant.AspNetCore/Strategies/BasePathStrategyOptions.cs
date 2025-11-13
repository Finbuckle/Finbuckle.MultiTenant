// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.AspNetCore.Options;

/// <summary>
/// Options for configuring the <see cref="Strategies.BasePathStrategy"/>.
/// </summary>
public class BasePathStrategyOptions
{
    /// <summary>
    /// Gets or sets whether to rebase the ASP.NET Core PathBase after tenant resolution.
    /// </summary>
    public bool RebaseAspNetCorePathBase { get; set; }
}
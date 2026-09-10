// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Options;

/// <summary>
/// Options for configuring when <see cref="MultiTenantMiddleware"/> should bypass tenant resolution
/// and pass the request directly to the next middleware.
/// </summary>
public class BypassWhenOptions
{
    private Func<HttpContext, bool>? _predicate;

    /// <summary>
    /// The callback that determines whether the current request should bypass tenant resolution.
    /// When <see langword="null"/>, bypass is not applied.
    /// </summary>
    public Func<HttpContext, bool>? Predicate
    {
        get => _predicate;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _predicate = value;
        }
    }
}


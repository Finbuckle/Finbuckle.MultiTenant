// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

//    Portions of this file are derived from the .NET Foundation source file located at:
//    https://github.com/dotnet/aspnetcore/blob/main/src/Http/Authentication.Core/src/AuthenticationSchemeProvider.cs

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore.Authentication;

/// <summary>
/// Implements <see cref="IAuthenticationSchemeProvider"/> as a decorator over an existing
/// <see cref="IAuthenticationSchemeProvider"/> registration (e.g. the default ASP.NET Core provider, or a
/// third-party dynamic provider such as Duende IdentityServer's). Scheme storage and lookup are fully
/// delegated to the decorated <c>inner</c> provider, which already owns that state at whatever lifetime it
/// was registered with. This class only adds per-tenant aware "default scheme" resolution on top, which is
/// why it reads <see cref="AuthenticationOptions"/> on every call. The options implementation remains
/// singleton-registered, with the ambient singleton <see cref="ITenantContext{TId}"/> selecting the current tenant.
/// </summary>
public class MultiTenantAuthenticationSchemeProvider : IAuthenticationSchemeProvider
{
    private readonly IAuthenticationSchemeProvider _inner;

    /// <summary>
    /// Creates an instance of <see cref="MultiTenantAuthenticationSchemeProvider"/>
    /// using the specified <paramref name="options"/> and decorates the existing <paramref name="inner"/>.
    /// </summary>
    /// <param name="inner">The <see cref="IAuthenticationSchemeProvider"/> to decorate.</param>
    /// <param name="options">The <see cref="AuthenticationOptions"/> options.</param>
    public MultiTenantAuthenticationSchemeProvider(IAuthenticationSchemeProvider inner,
        IOptions<AuthenticationOptions> options)
    {
        _inner = inner;
        _optionsProvider = options;
    }

    private readonly IOptions<AuthenticationOptions> _optionsProvider;

    private Task<AuthenticationScheme?> GetDefaultSchemeAsync()
        => _optionsProvider.Value.DefaultScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultScheme)
            : Task.FromResult<AuthenticationScheme?>(null);

    /// <summary>
    /// Returns the scheme for this tenant that will be used by default for <see cref="IAuthenticationService.AuthenticateAsync(HttpContext, string)"/>.
    /// This is typically specified via <see cref="AuthenticationOptions.DefaultAuthenticateScheme"/>.
    /// Otherwise, this will fall back to <see cref="AuthenticationOptions.DefaultScheme"/>.
    /// </summary>
    /// <returns>The scheme that will be used by default for <see cref="IAuthenticationService.AuthenticateAsync(HttpContext, string)"/> or null if not found.</returns>
    public virtual Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync()
        => _optionsProvider.Value.DefaultAuthenticateScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultAuthenticateScheme)
            : GetDefaultSchemeAsync();

    /// <summary>
    /// Returns the scheme for this tenant that will be used by default for <see cref="IAuthenticationService.ChallengeAsync(HttpContext, string, AuthenticationProperties)"/>.
    /// This is typically specified via <see cref="AuthenticationOptions.DefaultChallengeScheme"/>.
    /// Otherwise, this will fall back to <see cref="AuthenticationOptions.DefaultScheme"/>.
    /// </summary>
    /// <returns>The scheme that will be used by default for <see cref="IAuthenticationService.ChallengeAsync(HttpContext, string, AuthenticationProperties)"/> or null if not found.</returns>
    public virtual Task<AuthenticationScheme?> GetDefaultChallengeSchemeAsync()
        => _optionsProvider.Value.DefaultChallengeScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultChallengeScheme)
            : GetDefaultSchemeAsync();

    /// <summary>
    /// Returns the scheme for this tenant that will be used by default for <see cref="IAuthenticationService.ForbidAsync(HttpContext, string, AuthenticationProperties)"/>.
    /// This is typically specified via <see cref="AuthenticationOptions.DefaultForbidScheme"/>.
    /// Otherwise, this will fall back to <see cref="GetDefaultChallengeSchemeAsync"/> .
    /// </summary>
    /// <returns>The scheme that will be used by default for <see cref="IAuthenticationService.ForbidAsync(HttpContext, string, AuthenticationProperties)"/> or null if not found.</returns>
    public virtual Task<AuthenticationScheme?> GetDefaultForbidSchemeAsync()
        => _optionsProvider.Value.DefaultForbidScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultForbidScheme)
            : GetDefaultChallengeSchemeAsync();

    /// <summary>
    /// Returns the scheme for this tenant that will be used by default for <see cref="IAuthenticationService.SignInAsync(HttpContext, string, System.Security.Claims.ClaimsPrincipal, AuthenticationProperties)"/>.
    /// This is typically specified via <see cref="AuthenticationOptions.DefaultSignInScheme"/>.
    /// Otherwise, this will fall back to <see cref="AuthenticationOptions.DefaultScheme"/>.
    /// </summary>
    /// <returns>The scheme that will be used by default for <see cref="IAuthenticationService.SignInAsync(HttpContext, string, System.Security.Claims.ClaimsPrincipal, AuthenticationProperties)"/> or null if not found.</returns>
    public virtual Task<AuthenticationScheme?> GetDefaultSignInSchemeAsync()
        => _optionsProvider.Value.DefaultSignInScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultSignInScheme)
            : GetDefaultSchemeAsync();

    /// <summary>
    /// Returns the scheme for this tenant that will be used by default for <see cref="IAuthenticationService.SignOutAsync(HttpContext, string, AuthenticationProperties)"/>.
    /// This is typically specified via <see cref="AuthenticationOptions.DefaultSignOutScheme"/>.
    /// Otherwise, this will fall back to <see cref="GetDefaultSignInSchemeAsync"/> if that supports sign out.
    /// </summary>
    /// <returns>The scheme that will be used by default for <see cref="IAuthenticationService.SignOutAsync(HttpContext, string, AuthenticationProperties)"/> or null if not found.</returns>
    public virtual Task<AuthenticationScheme?> GetDefaultSignOutSchemeAsync()
        => _optionsProvider.Value.DefaultSignOutScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultSignOutScheme)
            : GetDefaultSignInSchemeAsync();

    /// <summary>
    /// Returns the <see cref="AuthenticationScheme"/> matching the name, or null.
    /// </summary>
    /// <param name="name">The name of the authenticationScheme.</param>
    /// <returns>The scheme or null if not found.</returns>
    public virtual Task<AuthenticationScheme?> GetSchemeAsync(string name)
        => _inner.GetSchemeAsync(name);

    /// <summary>
    /// Returns the scheme for this tenants in priority order for request handling.
    /// </summary>
    /// <returns>The schemes in priority order for request handling</returns>
    public virtual Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
        => _inner.GetRequestHandlerSchemesAsync();

    /// <summary>
    /// Registers a scheme for use by <see cref="IAuthenticationService"/>.
    /// </summary>
    /// <param name="scheme">The scheme.</param>
    public virtual void AddScheme(AuthenticationScheme scheme)
        => _inner.AddScheme(scheme);

    /// <summary>
    /// Removes a scheme, preventing it from being used by <see cref="IAuthenticationService"/>.
    /// </summary>
    /// <param name="name">The name of the authenticationScheme being removed.</param>
    public virtual void RemoveScheme(string name)
        => _inner.RemoveScheme(name);

    /// <summary>
    /// Returns all registered authentication schemes for this tenant.
    /// </summary>
    /// <returns>All registered authentication schemes.</returns>
    public virtual Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
        => _inner.GetAllSchemesAsync();
}

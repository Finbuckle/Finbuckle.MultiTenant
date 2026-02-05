// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

/// <summary>
/// A strategy that determines the tenant identifier from a claim in the user's identity.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ClaimStrategy : IMultiTenantStrategy
{
    private readonly string _tenantKey;
    private readonly string? _authenticationScheme;

    /// <summary>
    /// Initializes a new instance of ClaimStrategy.
    /// </summary>
    /// <param name="template">The claim type containing the tenant identifier.</param>
    public ClaimStrategy(string template) : this(template, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of ClaimStrategy.
    /// </summary>
    /// <param name="template">The claim type containing the tenant identifier.</param>
    /// <param name="authenticationScheme">The authentication scheme to use, or null for the default scheme.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="template"/> is null or whitespace.</exception>
    public ClaimStrategy(string template, string? authenticationScheme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        _tenantKey = template;
        _authenticationScheme = authenticationScheme;
    }

    /// <inheritdoc />
    public async Task<string?> GetIdentifierAsync(object context)
    {
        if (context is not HttpContext httpContext)
            return null;

        if (httpContext.User.Identity is { IsAuthenticated: true })
            return httpContext.User.FindFirst(_tenantKey)?.Value;

        AuthenticationScheme? authScheme;
        var schemeProvider = httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
        if (_authenticationScheme is null)
        {
            authScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync().ConfigureAwait(false);
        }
        else
        {
            authScheme =
                (await schemeProvider.GetAllSchemesAsync().ConfigureAwait(false)).FirstOrDefault(x =>
                    x.Name == _authenticationScheme);
        }

        if (authScheme is null)
        {
            return null;
        }

        var handler =
            (IAuthenticationHandler)ActivatorUtilities.CreateInstance(httpContext.RequestServices,
                authScheme.HandlerType);
        await handler.InitializeAsync(authScheme, httpContext).ConfigureAwait(false);
        httpContext.Items[$"{Constants.TenantToken}__bypass_validate_principal__"] = "true"; // Value doesn't matter.
        var handlerResult = await handler.AuthenticateAsync().ConfigureAwait(false);
        httpContext.Items.Remove($"{Constants.TenantToken}__bypass_validate_principal__");

        var identifier = handlerResult.Principal?.FindFirst(_tenantKey)?.Value;
        return identifier;
    }
}
// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Security.Claims;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore.Authentication;

/// <summary>
/// Multi-tenant aware authentication service that decorates the default authentication service.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class MultiTenantAuthenticationService<TTenantInfo> : IAuthenticationService
    where TTenantInfo : ITenantInfo
{
    private readonly IAuthenticationService _inner;
    private readonly IOptionsMonitor<MultiTenantAuthenticationOptions> _multiTenantAuthenticationOptions;

    /// <summary>
    /// Initializes a new instance of MultiTenantAuthenticationService.
    /// </summary>
    /// <param name="inner">The inner authentication service to decorate.</param>
    /// <param name="multiTenantAuthenticationOptions">The multi-tenant authentication options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is null.</exception>
    public MultiTenantAuthenticationService(IAuthenticationService inner,
        IOptionsMonitor<MultiTenantAuthenticationOptions> multiTenantAuthenticationOptions)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _multiTenantAuthenticationOptions = multiTenantAuthenticationOptions;
    }

    private static void AddTenantIdentifierToProperties(HttpContext context, ref AuthenticationProperties? properties)
    {
        // Add tenant identifier to the properties so on the callback we can use it to set the multi-tenant context.
        var multiTenantContext = context.GetMultiTenantContext<TTenantInfo>();
        if (multiTenantContext.TenantInfo != null)
        {
            properties ??= new AuthenticationProperties();
            if (!properties.Items.ContainsKey(Constants.TenantToken))
                properties.Items.Add(Constants.TenantToken, multiTenantContext.TenantInfo.Identifier);
        }
    }


    /// <inheritdoc />
    public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
        => _inner.AuthenticateAsync(context, scheme);

    /// <inheritdoc />
    public async Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        if (_multiTenantAuthenticationOptions.CurrentValue.SkipChallengeIfTenantNotResolved)
        {
            if (context.GetMultiTenantContext<TTenantInfo>().TenantInfo == null)
                return;
        }

        AddTenantIdentifierToProperties(context, ref properties);
        await _inner.ChallengeAsync(context, scheme, properties).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        AddTenantIdentifierToProperties(context, ref properties);
        await _inner.ForbidAsync(context, scheme, properties).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal,
        AuthenticationProperties? properties)
    {
        AddTenantIdentifierToProperties(context, ref properties);
        await _inner.SignInAsync(context, scheme, principal, properties).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        AddTenantIdentifierToProperties(context, ref properties);
        await _inner.SignOutAsync(context, scheme, properties).ConfigureAwait(false);
    }
}
// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

/// <summary>
/// A strategy that determines the tenant identifier from the state parameter during remote authentication callbacks.
/// </summary>
public class RemoteAuthenticationCallbackStrategy : IMultiTenantStrategy
{
    private readonly ILogger<RemoteAuthenticationCallbackStrategy> logger;

    /// <inheritdoc />
    public int Priority
    {
        get => -900;
    }

    /// <summary>
    /// Initializes a new instance of RemoteAuthenticationCallbackStrategy.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public RemoteAuthenticationCallbackStrategy(ILogger<RemoteAuthenticationCallbackStrategy> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc />
    public virtual async Task<string?> GetIdentifierAsync(object context)
    {
        if (context is not HttpContext httpContext)
            return null;

        var schemes = httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

        foreach (var scheme in (await schemes.GetRequestHandlerSchemesAsync().ConfigureAwait(false)).Where(s =>
                     typeof(IAuthenticationRequestHandler).IsAssignableFrom(s.HandlerType)))
        {
            // TODO verify this comment (still true as of net10.0)
            // Unfortunately we can't rely on the ShouldHandleAsync method since OpenId Connect handler doesn't use it.
            // Instead we'll get the paths to check from the options.
            var optionsType = scheme.HandlerType.GetProperty("Options")?.PropertyType;

            if (optionsType is null)
            {
                continue;
            }

            var optionsMonitorType = typeof(IOptionsMonitor<>).MakeGenericType(optionsType);
            var optionsMonitor = httpContext.RequestServices.GetRequiredService(optionsMonitorType);
            var options =
                optionsMonitorType?.GetMethod("Get")?.Invoke(optionsMonitor, new[] { scheme.Name }) as
                    RemoteAuthenticationOptions;

            if (options is null)
            {
                continue;
            }

            var callbackPath =
                (PathString)(optionsType.GetProperty("CallbackPath")?.GetValue(options) ?? PathString.Empty);
            var signedOutCallbackPath =
                (PathString)(optionsType.GetProperty("SignedOutCallbackPath")?.GetValue(options) ?? PathString.Empty);

            if (callbackPath.HasValue && callbackPath == httpContext.Request.Path ||
                signedOutCallbackPath.HasValue && signedOutCallbackPath == httpContext.Request.Path)
            {
                try
                {
                    string? state = null;

                    if (string.Equals(httpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                    {
                        state = httpContext.Request.Query["state"];
                    }
                    // Assumption: it is safe to read the form, limit to 1MB form size.
                    else if (string.Equals(httpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
                             && httpContext.Request.HasFormContentType
                             && httpContext.Request.Body.CanRead)
                    {
                        var formOptions = new FormOptions { BufferBody = true, MemoryBufferThreshold = 1048576 };

                        var form = await httpContext.Request.ReadFormAsync(formOptions).ConfigureAwait(false);
                        state = form.Single(i => string.Equals(i.Key, "state", StringComparison.OrdinalIgnoreCase))
                            .Value;
                    }

                    var properties = ((dynamic)options).StateDataFormat.Unprotect(state) as AuthenticationProperties;

                    if (properties == null)
                    {
                        if (logger != null)
                            logger.LogWarning(
                                "A tenant could not be determined because no state parameter passed with the remote authentication callback.");
                        return null;
                    }

                    properties.Items.TryGetValue(Constants.TenantToken, out var identifier);

                    return identifier;
                }
                catch (Exception e)
                {
                    throw new MultiTenantException("Error occurred resolving tenant for remote authentication.", e);
                }
            }
        }

        return null;
    }
}
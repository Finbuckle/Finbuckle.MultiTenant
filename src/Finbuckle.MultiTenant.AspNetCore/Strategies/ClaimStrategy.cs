// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant.Strategies
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class ClaimStrategy : IMultiTenantStrategy
	{
		private readonly string _tenantKey;
		private readonly string[]? _authenticationSchemes;

		public ClaimStrategy(string template) : this(template, null)
		{
		}

		public ClaimStrategy(string template, string[]? authenticationSchemes)
		{
			if (string.IsNullOrWhiteSpace(template))
				throw new ArgumentException(nameof(template));

			_tenantKey = template;
			_authenticationSchemes = authenticationSchemes;
		}

		public async Task<string?> GetIdentifierAsync(object context)
		{
            if (!(context is HttpContext httpContext))
                throw new MultiTenantException(null, new ArgumentException($@"""{nameof(context)}"" type must be of type HttpContext", nameof(context)));

            if (httpContext.User.Identity is { IsAuthenticated: true })
                return httpContext.User.FindFirst(_tenantKey)?.Value;

            var schemeProvider = httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

            if (_authenticationSchemes != null && _authenticationSchemes.Length > 0)
            {
                foreach (var schemeName in _authenticationSchemes)
                {
                    var authScheme = (await schemeProvider.GetAllSchemesAsync()).FirstOrDefault(x => x.Name == schemeName);
                    if (authScheme != null)
                    {
                        var identifier = await AuthenticateAndRetrieveIdentifier(httpContext, authScheme);
                        if (identifier != null) return identifier;
                    }
                }
            }
            else
            {
                var authScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();
                if (authScheme != null)
                {
                    var identifier = await AuthenticateAndRetrieveIdentifier(httpContext, authScheme);
                    if (identifier != null) return identifier;
                }
            }

            return null;
        }

        private async Task<string?> AuthenticateAndRetrieveIdentifier(HttpContext httpContext, AuthenticationScheme authScheme)
        {
            var handler = (IAuthenticationHandler)ActivatorUtilities.CreateInstance(httpContext.RequestServices, authScheme.HandlerType);
            await handler.InitializeAsync(authScheme, httpContext);
            httpContext.Items[$"{Constants.TenantToken}__bypass_validate_principal__"] = "true"; // Value doesn't matter.
            var handlerResult = await handler.AuthenticateAsync();
            httpContext.Items.Remove($"{Constants.TenantToken}__bypass_validate_principal__");

            return handlerResult.Principal?.FindFirst(_tenantKey)?.Value;
        }
    }
}

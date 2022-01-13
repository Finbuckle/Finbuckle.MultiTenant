// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

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
		private readonly string? _authenticationScheme;

		public ClaimStrategy(string template) : this(template, null)
		{
		}

		public ClaimStrategy(string template, string? authenticationScheme)
		{
			if (string.IsNullOrWhiteSpace(template))
				throw new ArgumentException(nameof(template));

			_tenantKey = template;
			_authenticationScheme = authenticationScheme;
		}

		public async Task<string?> GetIdentifierAsync(object context)
		{
			if (!(context is HttpContext httpContext))
				throw new MultiTenantException(null, new ArgumentException($@"""{nameof(context)}"" type must be of type HttpContext", nameof(context)));

			if (httpContext.User.Identity is { IsAuthenticated: true })
				return httpContext.User.FindFirst(_tenantKey)?.Value;

			AuthenticationScheme? authScheme;
			var schemeProvider = httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
			if (_authenticationScheme is null)
			{
				authScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();
			}
			else
			{
				authScheme = (await schemeProvider.GetAllSchemesAsync()).FirstOrDefault(x => x.Name == _authenticationScheme);
			}

			if (authScheme is null)
			{
				return null;
			}

			var handler = (IAuthenticationHandler)ActivatorUtilities.CreateInstance(httpContext.RequestServices, authScheme.HandlerType);
			await handler.InitializeAsync(authScheme, httpContext);
			httpContext.Items[$"{Constants.TenantToken}__bypass_validate_principal__"] = "true"; // Value doesn't matter.
			var handlerResult = await handler.AuthenticateAsync();
			httpContext.Items.Remove($"{Constants.TenantToken}__bypass_validate_principal__");

			var identifier = handlerResult.Principal?.FindFirst(_tenantKey)?.Value;
			return await Task.FromResult(identifier);
		}
	}
}

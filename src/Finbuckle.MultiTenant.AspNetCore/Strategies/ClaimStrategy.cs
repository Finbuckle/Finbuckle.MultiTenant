// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.Strategies
{
	public class ClaimStrategy : IMultiTenantStrategy
	{
		private readonly string _tenantKey;
		public ClaimStrategy(string template)
		{
			if (string.IsNullOrWhiteSpace(template))
				throw new ArgumentException(nameof(template));

			_tenantKey = template;
		}

		public async Task<string> GetIdentifierAsync(object context)
		{
			if (!(context is HttpContext httpContext))
				throw new MultiTenantException(null, new ArgumentException($@"""{nameof(context)}"" type must be of type HttpContext", nameof(context)));

			var schemeProvider = httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
			var authScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();
			
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

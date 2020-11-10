//    Copyright 2020 Finbuckle LLC, Andrew White, and Contributors
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

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
			httpContext.Items[$"{Constants.TenantToken}__bypass_validate_principle__"] = "true"; // Value doesn't matter.
			var handlerResult = await handler.AuthenticateAsync();
			httpContext.Items.Remove($"{Constants.TenantToken}__bypass_validate_principle__");

			var identifier = handlerResult.Principal?.FindFirst(_tenantKey)?.Value;
			return await Task.FromResult(identifier);
		}
	}
}

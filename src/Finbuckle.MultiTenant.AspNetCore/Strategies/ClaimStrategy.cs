using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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

			return await Task.FromResult(httpContext.User.FindFirst(_tenantKey)?.Value);
		}
	}
}

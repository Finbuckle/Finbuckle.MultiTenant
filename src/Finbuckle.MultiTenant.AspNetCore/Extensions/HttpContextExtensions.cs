using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// <c>Finbuckle.MultiTenant.AspNetCore</c> extensions to <c>HttpContext</c>.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Asyncronously retrieves the current <c>TenantContext</c> or null if there is no valid tenant context.
        /// </summary>
        /// <param name="context">The <c>HttpContext<c/> instance the extension method applies to.</param>
        /// <returns>The <c>TenantContext</c> instance for the current tenant.</returns>
        public static async Task<TenantContext> GetTenantContextAsync(this HttpContext context)
        {
            if (context.Items.TryGetValue(Constants.HttpContextTenantContext, out object tenantContext))
               return (TenantContext)tenantContext;

            var mw = new MultiTenantMiddleware(null);
            await mw.Invoke(context).ConfigureAwait((false));

            if (context.Items.TryGetValue(Constants.HttpContextTenantContext, out tenantContext))
                return (TenantContext)tenantContext;
            else
                return null;
        }
    }
}
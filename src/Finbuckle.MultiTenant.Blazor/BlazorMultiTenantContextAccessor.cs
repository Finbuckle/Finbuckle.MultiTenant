using Finbuckle.MultiTenant.Blazor;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant
{
    public class BlazorMultiTenantContextAccessor : IMultiTenantContextAccessor
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly TenantSession tenantSession;

        public BlazorMultiTenantContextAccessor(IHttpContextAccessor httpContextAccessor, TenantSession tenantSession)
        {
            this.httpContextAccessor = httpContextAccessor;

            this.tenantSession = tenantSession;
        }

        public IMultiTenantContext MultiTenantContext
        {
            get
            {
                var context = httpContextAccessor.HttpContext?.GetMultiTenantContext();

                if (context == null)
                {
                    if (this.tenantSession.TryGetValue(Constants.SessionStorageMultiTenantContext, out context))
                    {
                        return context;
                    }
                }

                return context;

            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Identity.Web;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.AzureFunctions.Bindings
{
    public class TenantValueProvider<TTenantInfo> : IValueProvider
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly HttpRequest _request;

        public TenantValueProvider(HttpRequest request)
        {
            _request = request;
        }

        public Type Type { get { return typeof(TTenantInfo); } }

        public Task<object> GetValueAsync()
        {
            if (_request.HttpContext.GetMultiTenantContext<TTenantInfo>() is null)
            {
                _request.HttpContext.TrySetTenantInfo<TTenantInfo>(default, true);
            }
            var tenantContext = _request.HttpContext.GetMultiTenantContext<TTenantInfo>();
            if (tenantContext is null)
            {
                return null;
            }
            var tenant = tenantContext.TenantInfo;
            return Task.FromResult((object)tenant);
        }

        public string ToInvokeString()
        {
            return string.Empty;
        }
    }
}
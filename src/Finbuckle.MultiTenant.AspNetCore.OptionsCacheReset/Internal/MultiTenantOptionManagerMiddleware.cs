using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore.OptionsCacheReset.Internal
{
    internal class MultiTenantOptionManagerMiddleware<TVersionTenantInfo> 
        where TVersionTenantInfo : class, IVersionTenantInfo, new()
    {
        private readonly RequestDelegate _next;
        private readonly TenantVersionStore _tenantVersionStore;
        private readonly IServiceProvider _serviceProvider;

        public MultiTenantOptionManagerMiddleware(
            RequestDelegate next,
            TenantVersionStore tenantVersionStore,
            IServiceProvider serviceProvider
        )
        {
            _next = next;
            _tenantVersionStore = tenantVersionStore;
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            var accessor = context.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TVersionTenantInfo>>();

            if (accessor.MultiTenantContext == null)
            {
                throw new InvalidOperationException(
                    "Use MultiTenantOptionManagerMiddleware After MultiTenantMiddleware");
            }

            var tenantInfo = accessor.MultiTenantContext.TenantInfo;
            var version = _tenantVersionStore.GetVersion(tenantInfo.Id);
            
            if (tenantInfo.Version != version)
                foreach (var tenantOptionMark in context.RequestServices.GetServices<MultiTenantOptionMark>())
                {
                    (_serviceProvider.GetRequiredService(tenantOptionMark.OptionsMonitorCacheOptionType) as
                        IOptionsMonitorCache<object>)?.Clear();
                    // https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/450
                    // (_serviceProvider.GetRequiredService(tenantOptionMark.OptionsCacheOptionType) as
                    //     MultiTenantOptionsManager<object>)?.Reset();
                    
                }            
            _tenantVersionStore.SetVersion(tenantInfo.Id, tenantInfo.Version);
            
            if (_next != null)
            {
                await _next(context);
            }
        }
    }
}
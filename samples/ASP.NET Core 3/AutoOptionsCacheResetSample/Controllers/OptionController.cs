using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AutoOptionsCacheResetSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OptionController : ControllerBase
    {
        private readonly IOptions<TestOption> _options;
        private readonly IMultiTenantStore<SampleTenantInfo> _tenantStore;
        public OptionController(IOptions<TestOption> options, IMultiTenantStore<SampleTenantInfo> tenantStore)
        {
            _options = options;
            _tenantStore = tenantStore;
        }

        [HttpGet]
        public TestOption Get()
        {
            return _options.Value;
        }
        
        [HttpPost]
        public async Task<int> Put()
        {
            var tenantInfo = await _tenantStore.TryGetByIdentifierAsync("finbuckle");
            tenantInfo.Version++;
            await _tenantStore.TryUpdateAsync(tenantInfo);
            return tenantInfo.Version;
        }
    }
}
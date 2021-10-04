// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HttpRemoteStoreSampleServer.Controllers
{
    [ApiController]
    [Route("[controller]/{identifier}")]
    public class TenantsController : ControllerBase
    {
        private readonly ILogger<TenantsController> _logger;
        private readonly IMultiTenantStore<TenantInfo> _store;

        public TenantsController(ILogger<TenantsController> logger, IMultiTenantStore<TenantInfo> store)
        {
            _logger = logger;
            this._store = store;
        }

        [HttpGet]
        public async Task<ActionResult<TenantInfo>> Get(string identifier)
        {
            _logger.LogInformation("Tenants endpoint called with identifier \"{identifier}\".", identifier);

            var tenantInfo = await _store.TryGetByIdentifierAsync(identifier);
            if(tenantInfo != null)
            {
                _logger.LogInformation("Tenant \"{name}\" found for identifier \"{identifier}\".", tenantInfo.Name, identifier);
                return tenantInfo;
            }

            _logger.LogWarning("No tenant found with identifier \"{identifier}\".", identifier);
            return NotFound();
        }
    }
}

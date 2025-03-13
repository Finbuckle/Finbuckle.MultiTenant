using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;

using MassTransit;

using MassTransitSample.Contracts;

using Microsoft.AspNetCore.Mvc;

namespace MassTransitSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestBusController : ControllerBase
    {
        private readonly IBus _bus;
        private readonly ILogger<TestBusController> _logger;
        private readonly IMultiTenantContextAccessor _mtca;

        public TestBusController(IBus bus, ILogger<TestBusController> logger, IMultiTenantContextAccessor mtca)
        {
            _bus = bus;
            _logger = logger;
            _mtca = mtca;
        }


        [HttpGet]
        public object Get()
        {
            _logger.LogInformation("Sending Bus Message for Tenant: {Tenant}", _mtca.MultiTenantContext.TenantInfo.Identifier);

            _bus.Publish(new HelloMessage { Text = "Hello, World!" });

            return new { triggered = true };
        }
    }
}

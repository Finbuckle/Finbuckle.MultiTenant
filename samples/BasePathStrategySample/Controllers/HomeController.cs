using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;

namespace BasePathStrategySample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var tc = HttpContext.GetMultiTenantContext()?.TenantInfo;
            return View(tc);
        }
    }
}

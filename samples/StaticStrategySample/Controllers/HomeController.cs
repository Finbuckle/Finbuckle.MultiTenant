using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace StaticStrategySample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var tc = HttpContext.GetTenantContextAsync().Result;
            return View(tc);
        }
    }
}

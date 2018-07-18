using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;

namespace HostStrategySample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var tc = HttpContext.GetTenantContext();
            return View(tc);
        }
    }
}
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace RouteStrategySample.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var tc = await HttpContext.GetTenantContextAsync();
            return View(tc);
        }
    }
}

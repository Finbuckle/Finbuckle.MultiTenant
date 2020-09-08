using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SessionStrategySample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var ti = HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
            return View(ti);
        }

        public IActionResult SetTenant(string identifier)
        {
            HttpContext.Session.SetString("__tenant__", identifier);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ClearTenant()
        {
            HttpContext.Session.Remove("__tenant__");
            return RedirectToAction(nameof(Index));
        }
    }
}

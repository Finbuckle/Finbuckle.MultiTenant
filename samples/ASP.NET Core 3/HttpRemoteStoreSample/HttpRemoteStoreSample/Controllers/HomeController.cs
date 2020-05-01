using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;

namespace HttpRemoteStoreSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var ti = HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
            return View(ti);
        }
    }
}

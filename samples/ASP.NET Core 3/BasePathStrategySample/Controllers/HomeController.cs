// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;

namespace BasePathStrategySample.Controllers
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

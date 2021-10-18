// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDataIsolationSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var ti = HttpContext.GetMultiTenantContext<SampleTenantInfo>()?.TenantInfo;
            return View(ti);
        }

        public IActionResult ReturnChallenge()
        {
            return Challenge();
        }

        public IActionResult ReturnForbid()
        {
            return Forbid();
        }
    }
}

using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DerivedTenantInfoSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<CustomOptions> optionsAccessor;

        public HomeController(IOptions<CustomOptions> optionsAccessor)
        {
            this.optionsAccessor = optionsAccessor;
        }

        public IActionResult Index()
        {
            var ti = HttpContext.GetMultiTenantContext()?.TenantInfo;

            var options = this.optionsAccessor.Value;

            var viewModel = new HomeViewModel { TenantInfo = ti, CustomOptions = options };

            return View(viewModel);
        }
    }
}

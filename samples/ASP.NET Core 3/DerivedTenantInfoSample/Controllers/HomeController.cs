using DerivedTenantInfoSample.Services;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace DerivedTenantInfoSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly CustomService customService;

        private readonly IOptions<CustomOptions> optionsAccessor;

        public HomeController(IOptions<CustomOptions> optionsAccessor, CustomService customService)
        {
            this.customService = customService;

            this.optionsAccessor = optionsAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var ti = HttpContext.GetMultiTenantContext()?.TenantInfo;

            // Options are not null
            var optionsFromContext = this.optionsAccessor.Value;

            // Options are null
            var optionsFromService = await this.customService.GetOptions();

            var viewModel = new HomeViewModel { TenantInfo = ti, CustomOptions = optionsFromService };

            return View(viewModel);
        }
    }
}

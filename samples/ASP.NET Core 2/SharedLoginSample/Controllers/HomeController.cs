using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SharedLoginSample.Models;

namespace SharedLoginSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMultiTenantStore<TenantInfo> store;

        public HomeController(IMultiTenantStore<TenantInfo> store)
        {
            this.store = store;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SharedLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SharedLogin(SharedLoginModel model)
        {
            if (ModelState.IsValid)
            {
                // We will use the email address domain to find the tenant
                // identifier using a simple dictionary mapping here. 

                // We could just set up the multitenant store
                // so that the email domain is the identifier, but that can
                // interfere route or host if those multitenant strategies
                // are used.

                // In a real application you might query a database query or
                // call an API to get the tenant identifier from the email
                // domain.

                var tenantDomainMap = new Dictionary<string, string>()
                {
                    {"finbuckle.com", "finbuckle"},
                    {"megacorp.com", "megacorp"},
                    { "initech.com", "initech"}
                };

                var domain = model.Email.Substring(model.Email.IndexOf("@") + 1).ToLower();
                tenantDomainMap.TryGetValue(domain, out var identifier);
                if (identifier == null)
                {
                    ModelState.TryAddModelError("", "Tenant not found.");
                    goto SkipToEnd;
                }

                // Get the actual tenant informtion.
                var tenantInfo = await store.TryGetByIdentifierAsync(identifier);

                if (tenantInfo == null)
                {
                    ModelState.TryAddModelError("", "Tenant not found.");
                    goto SkipToEnd;
                }

                // Save the original TenantInfo and service provider.
                var originalTenantInfo = HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
                var orignalServiceProvider = HttpContext.RequestServices;

                // Set the new TenantInfo and reset the service provider.
                HttpContext.TrySetTenantInfo(tenantInfo, resetServiceProviderScope: true);
                
                // Now sign in and redirect (using Identity in this example). Since TenantInfo is set the options that the signin
                // uses internally will be for this tenant.
                var signInManager = HttpContext.RequestServices 
                                        .GetRequiredService<SignInManager<IdentityUser>>();
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Home", new { __tenant__ = tenantInfo.Identifier });
                else
                    ModelState.TryAddModelError("", "User not found.");

                // In case an error signing in, reset the TenantInfo and ServiceProvider so that the
                // view in unaffected.
                HttpContext.RequestServices = orignalServiceProvider;
                HttpContext.TrySetTenantInfo(originalTenantInfo, resetServiceProviderScope: false);
            }

        // We should only reach this if there was a problem signing in. The view will dispay the errors.
        SkipToEnd:
            model.Password = "";
            return View(model);
        }
    }
}

using System.Security.Claims;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PerTenantAuthenticationSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var ti = HttpContext.GetMultiTenantContext<SampleTenantInfo>()?.TenantInfo;
            var title = (ti?.Name ?? "No tenant") + " - ";

            ViewData["style"] = "navbar-light bg-light";

            if (!User.Identity.IsAuthenticated)
            {
                title += "Not Authenticated";
            }
            else
            {
                title += "Authenticated";
                ViewData["style"] = "navbar-dark bg-dark";
            }

            ViewData["Title"] = title;

            if (ti != null)
            {
                var cookieOptionsMonitor = HttpContext.RequestServices.GetService<IOptionsMonitor<CookieAuthenticationOptions>>();
                var cookieName = cookieOptionsMonitor.Get(CookieAuthenticationDefaults.AuthenticationScheme).Cookie.Name;
                ViewData["CookieName"] = cookieName;

                var schemes = HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                ViewData["ChallengeScheme"] = schemes.GetDefaultChallengeSchemeAsync().Result.Name;
            }

            return View(ti);
        }

        [Authorize]
        public IActionResult Authenticate()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync();
            await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Username") }, "Cookies")));
  
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index");
        }
    }
}

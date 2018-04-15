using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthenticationOptionsSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var tc = HttpContext.GetTenantContextAsync().Result;   
            var title = (tc?.Name ?? "No tenant") + " - ";

            if(!User.Identity.IsAuthenticated)
            {
                title += "Not Authenticated";
            }
            else
            {
                title += "Authenticated";
            }

            ViewData["Title"] = title;

            var cookieOptionsMonitor = (IOptionsMonitor<CookieAuthenticationOptions>)HttpContext.RequestServices.GetService(typeof(IOptionsMonitor<CookieAuthenticationOptions>));
            var cookieName = cookieOptionsMonitor.Get(CookieAuthenticationDefaults.AuthenticationScheme).Cookie.Name;
            ViewData["CookieName"] = cookieName;
            
            return View(tc);
        }

        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync();
            await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim(ClaimTypes.Name, "Username") }, "Cookie")));
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}

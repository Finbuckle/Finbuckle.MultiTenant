using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDataIsolationSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

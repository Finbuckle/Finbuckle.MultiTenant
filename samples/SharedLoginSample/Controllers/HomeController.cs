using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SharedLoginSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

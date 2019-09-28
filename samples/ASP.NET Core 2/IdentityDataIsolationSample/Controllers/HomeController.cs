﻿using System.Diagnostics;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDataIsolationSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(HttpContext.GetMultiTenantContext()?.TenantInfo);
        }
    }
}

﻿using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;

namespace HostStrategySample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var ti = HttpContext.GetMultiTenantContext()?.TenantInfo;
            return View(ti);
        }
    }
}
// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Linq;
using DataIsolationSample.Data;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using DataIsolationSample.Models;

namespace DataIsolationSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ToDoDbContext _dbContext;

        public HomeController(ToDoDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public IActionResult Index()
        {
            // Get the list of to do items. This will only return items for the current tenant.
            IEnumerable<ToDoItem> toDoItems = null;
            if(HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo != null)
            {
                toDoItems = _dbContext.ToDoItems.ToList();
            }

            return View(toDoItems);
        }
    }
}

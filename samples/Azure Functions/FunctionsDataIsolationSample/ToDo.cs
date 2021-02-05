using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Finbuckle.MultiTenant;
using System.Collections;
using FunctionsDataIsolationSample.Models;
using System.Collections.Generic;
using FunctionsDataIsolationSample.Data;
using System.Linq;

namespace FunctionsDataIsolationSample
{
    public class ToDo
    {
        private readonly ToDoDbContext _dbContext;

        public ToDo(ToDoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [FunctionName("ToDo")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{tenant}/ToDo")] HttpRequest req,
            string tenant,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation($"Route Tenant: {tenant}");

            IEnumerable<ToDoItem> toDoItems = null;

            var ti = req.HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
            if (ti is null)
            {
                log.LogInformation("No tenant found.");
            }
            else
            {
                log.LogInformation($"Tenant Information: {ti.Id}, {ti.Name}, {ti.Identifier}, {ti.ConnectionString}");
                toDoItems = _dbContext.ToDoItems.ToList();
            }

            return new OkObjectResult(toDoItems);
        }
    }
}

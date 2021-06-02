using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AzureFunctions;

using FunctionsDataIsolationSample.Data;
using FunctionsDataIsolationSample.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            [Tenant]TenantInfo tenant,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation($"Route Tenant: {tenant}");

            IEnumerable<ToDoItem> toDoItems = null;

            if (tenant is null)
            {
                log.LogInformation("No tenant found.");
            }
            else
            {
                log.LogInformation($"Tenant Information: {tenant.Id}, {tenant.Name}, {tenant.Identifier}, {tenant.ConnectionString}");
                toDoItems = _dbContext.ToDoItems.ToList();
            }

            return new OkObjectResult(toDoItems);
        }
    }
}

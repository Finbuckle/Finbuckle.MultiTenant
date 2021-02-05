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

namespace FunctionsBasePathStrategySample
{
    public static class Test
    {
        [FunctionName("Test")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{tenant}/Test")] HttpRequest req,
            string tenant,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation($"Route Tenant: {tenant}");

            string name = req.Query["name"];

            var ti = req.HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
            if (ti is null)
            {
                log.LogInformation("No tenant found.");
            }
            else
            {
                log.LogInformation($"Tenant Information: {ti.Id}, {ti.Name}, {ti.Identifier}, {ti.ConnectionString}");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

using System;
using System.Threading.Tasks;

namespace FunctionsClaimStrategySample.Extensions
{
    public static class HttpRequestAuthExtensions
    {
        public static async Task<(bool Succeeded, IActionResult? ActionResult)> ValidateAuthAsync(this HttpRequest request, ILogger log)
        {
            try
            {
                if (!request.HttpContext.User.Identity.IsAuthenticated)
                {
                    (bool Succeeded, IActionResult? ActionResult) result = await request.HttpContext.AuthenticateAzureFunctionAsync().ConfigureAwait(false);
                    if (result.Succeeded)
                    {
                        log.LogInformation("Authentication Succeeded.");
                        log.LogInformation($"Authenticated HttpContext: `{request.HttpContext.User.Identity.IsAuthenticated}`");
                        return (true, null);
                    }
                    else
                    {
                        log.LogInformation("Authentication Failed.");
                        return (false, result.ActionResult);
                    }
                }
                else
                {
                    log.LogInformation("User already authenticated.");
                    log.LogInformation($"Authenticated HttpContext: `{request.HttpContext.User.Identity.IsAuthenticated}`");
                    if (request.HttpContext.User.Identity.IsAuthenticated)
                    {
                        return (true, null);
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError("ValidateAuth: {0}", e.Message);
            }
            return (false, new UnauthorizedResult());
        }
    }

    public class HttpRequestAuthResponse
    {
        public bool IsValid { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Core
{
    public class Utilities
    {
        public static void TryLogInfo(ILogger logger, string message)
        {
            if (logger != null)
            {
                logger.LogInformation(message);
            }
        }
    }
}
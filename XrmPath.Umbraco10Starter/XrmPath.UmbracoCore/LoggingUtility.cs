using Microsoft.Extensions.Logging;
using XrmPath.UmbracoCore.BaseServices;
using XrmPath.UmbracoCore.Utilities;

namespace XrmPath.UmbracoCore
{
    /// <summary>
    /// Dependencies: Logger
    /// </summary>
    /// <param name="serviceUtil"></param>
    public class LoggingUtility: BaseInitializer
    {
        public LoggingUtility(ServiceUtility? serviceUtil) : base(serviceUtil){}

        public void Information(string message)
        {
            logger?.LogInformation(message);
        }
        public void Warning(string message)
        {
            logger?.LogWarning(message);
        }
        public void Error(string message, Exception ex)
        {
            logger?.LogError(message);
        }
    }
}

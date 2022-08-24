using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmPath.UmbracoCore.Utilities;

namespace XrmPath.UmbracoCore
{
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

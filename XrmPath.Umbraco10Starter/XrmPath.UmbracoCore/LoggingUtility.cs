using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmPath.UmbracoCore
{
    public class LoggingUtility
    {
        private readonly ILogger<object> _logger;

        public LoggingUtility(ILogger<object> logger)
        {
            _logger = logger;
        }

        public void Information(string message)
        {
            _logger.LogInformation(message);
        }
        public void Warning(string message)
        {
            _logger.LogWarning(message);
        }

        public void Error(string message, Exception ex)
        {
            _logger.LogError(message);
        }
    }
}

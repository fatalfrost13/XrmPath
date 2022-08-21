using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;



namespace XrmPath.UmbracoCore.Helpers
{
    public static class LogHelper
    {
        public static void Information(string message)
        {
            Current.Logger.Info<string>(message);
        }
        public static void Warning(string message)
        {
            Current.Logger.Warn<string>(message);
        }

        public static void Error(string message, Exception ex)
        {
            Current.Logger.Error<string>(ex, message);
        }
    }
}

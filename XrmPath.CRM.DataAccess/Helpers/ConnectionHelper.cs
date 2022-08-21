using System;
using System.Configuration;
using System.IO;
using System.Net;
using XrmPath.CRM.DataAccess.Helpers.Utilities;
using log4net;

namespace XrmPath.CRM.DataAccess.Helpers
{
    /// <summary>
    /// This class is used between Helper classes to initialize EntityUtility/CrmUtility and only create 1 instance of it
    /// Utility will handle expiration of SDK Tokens.
    /// This should only be used for GET methods and read only lists
    /// Double lock check to ensure these objects are threadsafe
    /// </summary>
    public static class ConnectionHelper
    {
        public static ILog LogHelper = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object LockEntityUtility = new object();
        private static readonly object LockCrmUtility = new object();

        private static EntityUtility _entityUtility;
        public static EntityUtility EntityUtility
        {
            get
            {
                if (_entityUtility == null)
                {
                    lock (LockEntityUtility)
                    {
                        if (_entityUtility == null)
                        {
                            _entityUtility = new EntityUtility();
                        }
                    }
                }
                return _entityUtility;
            }
        }
        private static CrmUtility _crmUtility;
        public static CrmUtility CrmUtility
        {
            get
            {
                if (_crmUtility == null)
                {
                    lock (LockCrmUtility)
                    {
                        if (_crmUtility == null)
                        {
                            _crmUtility = new CrmUtility();
                        }
                    }
                }
                return _crmUtility;
            }
        }
    }
}

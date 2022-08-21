using System;
using System.Collections.Generic;
using XrmPath.CRM.DataAccess.Helpers.Utilities;
using XrmPath.CRM.DataAccess.Models;
using log4net;

namespace XrmPath.CRM.DataAccess.Helpers
{
    public static class OptionSetHelper
    {
        public static ILog LogHelper = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static EntityUtility _entityUtility;
        public static EntityUtility EntityUtility
        {
            get
            {
                if (_entityUtility == null)
                {
                    _entityUtility = new EntityUtility();
                }
                return _entityUtility;
            }
        }
        public static List<OptionSetModel> GetOptionSetList(string entityName, string attributeName)
        {
            var optionSetList = new List<OptionSetModel>();
            try
            {
                var optionSets = EntityUtility.service.GetOptionSetLabelsAndValues(entityName, attributeName);
                foreach (var set in optionSets)
                {
                    optionSetList.Add(new OptionSetModel
                    {
                        Id = set.Key,
                        Value = set.Value
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"XrmPath.CRM.DataAccess caught error on OptionSetHelper.GetOptionSetList()", ex);
            }

            return optionSetList;
        }

        
    }
}

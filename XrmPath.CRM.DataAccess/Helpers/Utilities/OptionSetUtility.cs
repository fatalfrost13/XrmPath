using System;
using System.Collections.Generic;
using System.Linq;
using XrmPath.CRM.DataAccess.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace XrmPath.CRM.DataAccess.Helpers.Utilities
{
    public static class OptionSetUtility
    {
        public const int MissingValue = -1;

        /// <summary>
        /// T1 = entityName
        /// T2 = attributeName
        /// T3 = Options
        /// </summary>
        private static List<Tuple<string, string, Dictionary<int, string>>> OptionSets = new List<Tuple<string, string, Dictionary<int, string>>>();

        /// <summary>
        /// Return the Labels and Values for an Option Set attribute on an Entity.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entityName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static Dictionary<int, string> GetOptionSetLabelsAndValues(this IOrganizationService service, string entityName, string attributeName)
        {
            var optionSet = OptionSets.Find(o => o.Item1 == entityName && o.Item2 == attributeName);
            if (optionSet == null)
            {
                var request = new RetrieveAttributeRequest
                {
                    EntityLogicalName = entityName,
                    LogicalName = attributeName,
                    RetrieveAsIfPublished = true
                };
                var response = (RetrieveAttributeResponse)service.Execute(request);
                var pairs = ((PicklistAttributeMetadata)response.AttributeMetadata).OptionSet.Options;
                var dictionary = new Dictionary<int, string>();
                foreach (var pair in pairs)
                {
                    dictionary.Add(pair.Value.Value, pair.Label.UserLocalizedLabel.Label);
                }
                OptionSets.Add(new Tuple<string, string, Dictionary<int, string>>(entityName, attributeName, dictionary));
                return dictionary;
            }
            else
            {
                return optionSet.Item3;
            }
        }

        public static int GetOptionSetValueByLabel(this IOrganizationService service, string entityName, string attributeName, string label)
        {
            var optionSet = OptionSets.Find(o => o.Item1 == entityName && o.Item2 == attributeName);
            if (optionSet == null)
            {
                GetOptionSetLabelsAndValues(service, entityName, attributeName);
                optionSet = OptionSets.Find(o => o.Item1 == entityName && o.Item2 == attributeName);
            }

            var option = optionSet.Item3.FirstOrDefault(o => o.Value == label);
            if (option.Key == 0)
            {
                throw new ApplicationException(string.Format("OptionSet value of label {0} could not be found.", label));
            }
            return option.Key;
        }

        public static int? GetOptionSetValueByLabelOrNull(this IOrganizationService service, string entityName, string attributeName, string label)
        {
            var optionSet = OptionSets.Find(o => o.Item1 == entityName && o.Item2 == attributeName);
            if (optionSet == null)
            {
                GetOptionSetLabelsAndValues(service, entityName, attributeName);
                optionSet = OptionSets.Find(o => o.Item1 == entityName && o.Item2 == attributeName);
            }

            var option = optionSet.Item3.FirstOrDefault(o => o.Value == label);
            if (option.Key == 0)
            {
                return null;
            }
            return option.Key;
        }

        public static string GetOptionSetValueLabel(this IOrganizationService service, Entity entity, string attribute, OptionSetValue option)
        {
            string optionLabel = String.Empty;
            RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entity.LogicalName,
                LogicalName = attribute,
                RetrieveAsIfPublished = true
            };
            RetrieveAttributeResponse attributeResponse = (RetrieveAttributeResponse)service.Execute(attributeRequest);
            AttributeMetadata attrMetadata = (AttributeMetadata)attributeResponse.AttributeMetadata;
            PicklistAttributeMetadata picklistMetadata = (PicklistAttributeMetadata)attrMetadata;

            // For every status code value within all of our status codes values    
            //  (all of the values in the drop down list)    
            foreach (OptionMetadata optionMeta in picklistMetadata.OptionSet.Options)
            {        // Check to see if our current value matches        
                if (optionMeta.Value == option.Value)
                {
                    // If our numeric value matches, set the string to our status code            
                    //  label            
                    optionLabel = optionMeta.Label.UserLocalizedLabel.Label;
                }
            }

            return optionLabel;
        }

        public static OptionSetModel GetOptionSetModel(this IOrganizationService service, string entityName, string fieldName, int optionSetValue)
        {

            var attReq = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = fieldName,
                RetrieveAsIfPublished = true
            };

            var attResponse = (RetrieveAttributeResponse)service.Execute(attReq);
            var attMetadata = (EnumAttributeMetadata)attResponse.AttributeMetadata;

            OptionSetModel optionSetModel = null;
            var label = attMetadata.OptionSet.Options.FirstOrDefault(x => x.Value == optionSetValue)?.Label.UserLocalizedLabel.Label;
            var id = optionSetValue;

            optionSetModel = new OptionSetModel
            {
                Id = id,
                Value = label
            };

            return optionSetModel;

        }

        public static Dictionary<int, string> GetStatusLabelsAndValues(this IOrganizationService service, string entityName, bool includeInactive)
        {
            var optionSet = OptionSets.Find(o => o.Item1 == entityName && o.Item2 == "statuscode");
            if (optionSet == null)
            {
                var request = new RetrieveAttributeRequest
                {
                    EntityLogicalName = entityName,
                    LogicalName = "statuscode",
                    RetrieveAsIfPublished = true
                };
                var response = (RetrieveAttributeResponse)service.Execute(request);
                var pairs = ((StatusAttributeMetadata)response.AttributeMetadata).OptionSet.Options;
                var dictionary = new Dictionary<int, string>();
                foreach (StatusOptionMetadata pair in pairs)
                {
                    if (pair.State == 0 || includeInactive)
                    { dictionary.Add(pair.Value.Value, pair.Label.UserLocalizedLabel.Label); }
                }
                OptionSets.Add(new Tuple<string, string, Dictionary<int, string>>(entityName, "statuscode", dictionary));
                return dictionary;
            }
            else
            {
                return optionSet.Item3;
            }
        }

        public static int GetStatusValueByLabel(this IOrganizationService service, string entityName, string label)
        {
            var optionSet = OptionSets.Find(o => o.Item1 == entityName && o.Item2 == "statuscode");
            if (optionSet == null)
            {
                GetStatusLabelsAndValues(service, entityName, true);
                optionSet = OptionSets.Find(o => o.Item1 == entityName && o.Item2 == "statuscode");
            }

            var option = optionSet.Item3.FirstOrDefault(o => o.Value == label);
            if (option.Key == 0)
            {
                throw new ApplicationException(string.Format("OptionSet value of label {0} could not be found.", label));
            }
            return option.Key;
        }

        public static string GetStatusValueLabel(this IOrganizationService service, Entity entity, OptionSetValue option)
        {
            string optionLabel = String.Empty;
            RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entity.LogicalName,
                LogicalName = "statuscode",
                RetrieveAsIfPublished = true
            };
            RetrieveAttributeResponse attributeResponse = (RetrieveAttributeResponse)service.Execute(attributeRequest);
            AttributeMetadata attrMetadata = (AttributeMetadata)attributeResponse.AttributeMetadata;
            StatusAttributeMetadata picklistMetadata = (StatusAttributeMetadata)attrMetadata;

            // For every status code value within all of our status codes values    
            //  (all of the values in the drop down list)    
            foreach (StatusOptionMetadata optionMeta in picklistMetadata.OptionSet.Options)
            {        // Check to see if our current value matches        
                if (optionMeta.Value == option.Value)
                {
                    // If our numeric value matches, set the string to our status code            
                    //  label            
                    optionLabel = optionMeta.Label.UserLocalizedLabel.Label;
                }
            }

            return optionLabel;
        }

        public static bool GetOptionSetBoolean(this IOrganizationService service, Entity entity, string attribute, OptionSetValue option)
        {
            var boolvalue = false;
            var value = GetOptionSetValueLabel(service, entity, attribute, option);
            var formattedValue = value.ToLower().Trim();
            if (formattedValue == "1" || formattedValue == "true" || formattedValue == "yes")
            {
                boolvalue = true;
            }
            return boolvalue;
        }

        public static bool GetSimpleOptionSetBoolean(OptionSetValue optionSetValue)
        {
            var isTrue = optionSetValue?.Value == GlobalBooleanOptionSet.True;
            return isTrue;
        }

        public static OptionSetValue GetSimpleOptionSetValue(bool isTrue)
        {
            var value = isTrue ? GlobalBooleanOptionSet.True : GlobalBooleanOptionSet.False;
            return new OptionSetValue(value);
        }
    }
}

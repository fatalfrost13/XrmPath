using XrmPath.CRM.DataAccess.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using log4net;
using System.Dynamic;
using Microsoft.Xrm.Sdk;

namespace XrmPath.CRM.DataAccess.Helpers
{
    public static class TransformHelper
    {
        public static ILog LogHelper = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This method will go through each field in an object, and transform it into a DynamicEntityModel bassed on field name in JsonPropertyAttribute.
        /// </summary>
        /// <param name="dynamic"></param>
        /// <returns></returns>
        public static CrmDynamicEntityModel TransformToEntity<T>(T dynamic)
        {
            var entity = new CrmDynamicEntityModel();
            try
            {
                entity.Id = Guid.Empty;
                entity.Name = string.Empty;

                var entityNameAttribute = dynamic.GetType().GetCustomAttributes(true).FirstOrDefault(i => i.GetType() == typeof(CrmDynamicEntityAttribute));
                if (entityNameAttribute != null)
                {
                    entity.Name = (entityNameAttribute as CrmDynamicEntityAttribute).EntityName;
                }

                var properties = dynamic.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var fieldType = prop.PropertyType;
                    var fieldName = prop.Name;
                    var fieldValue = prop.GetValue(dynamic);
                    var fieldReadOnly = false;

                    var crmFieldName = string.Empty;
                    var crmForceType = string.Empty;
                    var skipValueCheck = (string)null;
                    var skipAttribute = false;
                    var fieldRegardingEntity = string.Empty;

                    var dynamicEntityAttribute = prop.GetCustomAttributes(true).FirstOrDefault(i => i.GetType() == typeof(CrmDynamicEntityFieldAttribute));
                    if (dynamicEntityAttribute != null)
                    {
                        //set field name
                        crmFieldName = (dynamicEntityAttribute as CrmDynamicEntityFieldAttribute).EntityFieldName;
                        fieldReadOnly = (dynamicEntityAttribute as CrmDynamicEntityFieldAttribute).ReadOnly;
                        crmForceType = (dynamicEntityAttribute as CrmDynamicEntityFieldAttribute).Type;
                        fieldRegardingEntity = (dynamicEntityAttribute as CrmDynamicEntityFieldAttribute).RegardingObjectEntity;

                        //validate crmForceType
                        if (!string.IsNullOrEmpty(crmForceType) && !CrmDynamicEntityHelper.ValidFieldTypesList.Contains(crmForceType))
                        {
                            //invalid crmForceType. set it back to empty
                            crmForceType = string.Empty;
                        }

                        skipValueCheck = (dynamicEntityAttribute as CrmDynamicEntityFieldAttribute).SkipSaveIfValueEquals;
                        if (skipValueCheck != null && fieldValue != null)
                        {
                            if (skipValueCheck.ToString().Equals(fieldValue.ToString()))
                            {
                                skipAttribute = true;
                            }
                        }

                        var isPrimaryKey = (dynamicEntityAttribute as CrmDynamicEntityFieldAttribute).IsPrimaryKey;
                        if (isPrimaryKey && fieldValue != null)
                        {
                            bool validGuid;
                            Guid entityId;
                            validGuid = Guid.TryParse(fieldValue?.ToString(), out entityId);
                            if (!validGuid)
                            {
                                entity.Id = Guid.Empty;
                            }
                            else
                            {
                                entity.Id = entityId;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(crmFieldName))
                    {
                        //if crmFieldName is not assigned, use the name in the JsonPropertyAttribute
                        var jsonAttribute = prop.GetCustomAttributes(true).FirstOrDefault(i => i.GetType() == typeof(JsonPropertyAttribute));
                        if (jsonAttribute != null)
                        {
                            crmFieldName = (jsonAttribute as JsonPropertyAttribute).PropertyName;
                        }
                        if (string.IsNullOrEmpty(crmFieldName))
                        {
                            var dataMemberAttribute = prop.GetCustomAttributes(true).FirstOrDefault(i => i.GetType() == typeof(DataMemberAttribute));
                            if (dataMemberAttribute != null)
                            {
                                //if it's still empty, check DataMember property
                                crmFieldName = (dataMemberAttribute as DataMemberAttribute).Name;
                            }
                        }
                    }

                    if (fieldValue != null && !string.IsNullOrEmpty(crmFieldName) && !skipAttribute && !fieldReadOnly)
                    {
                        var crmType = !string.IsNullOrEmpty(crmForceType) ? crmForceType : CrmDynamicEntityHelper.GetCrmFieldType(fieldType, fieldRegardingEntity);
                        fieldValue = CrmDynamicEntityHelper.GetValueByType(fieldValue, crmType, fieldRegardingEntity); //format the value to the correct data type.

                        var dynamicEntityField = new CrmDynamicEntityField
                        {
                            Name = crmFieldName,
                            Value = fieldValue,
                            Type = crmType,
                            RegardingEntity = fieldRegardingEntity
                        };
                        entity.FieldList.Add(dynamicEntityField);
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error($"XrmPath.CRM.DataAccess caught error on TransformHelper.TransformToEntity()", ex);
                throw ex;
            }
            return entity;
        }
        public static T ToModel<T>(this CrmDynamicEntityModel entityModel)
        {
            //var serializedData = JsonConvert.SerializeObject(entityModel);
            //var dynamicObject = Activator.CreateInstance<T>();
            dynamic expando = new ExpandoObject();
            foreach (var dynamicField in entityModel.FieldList)
            {

                var fieldName = dynamicField.Name;
                //var attribute = CrmDynamicEntityHelper.GetAttributeValue(dynamicField);
                var attribute = CrmDynamicEntityHelper.GetValueByType(dynamicField.Value, dynamicField.Type, dynamicField.RegardingEntity);
                
                if (dynamicField.Type == "Currency" || dynamicField.Type == "Money") {
                    attribute = ((Money)attribute).Value;
                }
                else if (dynamicField.Type == "EntityReference")
                {
                    attribute = ((EntityReference)attribute).Id;
                }
                else if (dynamicField.Type == "OptionSetValue")
                {
                    attribute = ((OptionSetValue)attribute).Value;
                }

                AddExpandoProperty(expando, fieldName, attribute);
                //entity.Attributes[dynamicField.Name] = attribute;
            }

            var serialize = JsonConvert.SerializeObject(expando);
            var returnObj = JsonConvert.DeserializeObject<T>(serialize);
            return returnObj;
        }

        private static void AddExpandoProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            //https://www.oreilly.com/learning/building-c-objects-dynamically
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
            {
                expandoDict[propertyName] = propertyValue;
            }

            else {
                expandoDict.Add(propertyName, propertyValue);
            }
        }
    }
}

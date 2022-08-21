using XrmPath.CRM.DataAccess.Model;
using XrmPath.CRM.DataAccess.Models;
using XrmPath.CRM.DataAccess.Utilities;
using log4net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;

namespace XrmPath.CRM.DataAccess.Helpers
{


    public static class CrmDynamicEntityHelper
    {
        public static ILog LogHelper = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string ValidFieldTypes = "String,OptionSetValue,Guid,Double,Decimal,Boolean,Currency,Money,Int32,DateTime,EntityReference";
        public static ISet<string> ValidFieldTypesList = ValidFieldTypes.StringToSet();

        /// <summary>
        /// This will make a request to CRM API and pull the data based on entity ID and columns requests.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static CrmDynamicEntityModel GetDynamicEntity(string entityName, Guid entityId, string columns = null)
        {
            var dynamicModel = new CrmDynamicEntityModel();
            try
            { 
                var columnSet = new ColumnSet(true);

                if (columns != null) {
                    var columnList = columns.StringToSet().Select(i => i.Trim()).ToList();
                    columnSet = new ColumnSet(columnList.ToArray());
                }

                var result = ConnectionHelper.EntityUtility.service.Retrieve(entityName, entityId, columnSet);
                
                if (result != null) {
                    dynamicModel = result.EntityToDynamicModel();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"XrmPath.CRM.DataAccess caught error on CrmDynamicEntityHelper.GetDynamicEntity()", ex);
            }
            return dynamicModel;
        }


        /// <summary>
        /// Pulls all records and selected columns of the requested entity
        /// </summary>
        /// <param name="entityName">Entity Name</param>
        /// <param name="columns">Comma separated string of columns to return</param>
        /// <param name="filterExpressionList">List of FilterExpressions</param>
        /// <param name="orderExpressionList">List of OrderExpressions</param>
        /// <returns></returns>
        public static List<CrmDynamicEntityModel> GetDynamicEntityListAdvanced(string entityName, string columns = null, List<FilterExpression> filterExpressionList = null, List<OrderExpression> orderExpressionList = null, List<JoinExpression> joinExpressionList = null)
        {

            var query = new QueryExpression() { };
            var columnSet = new ColumnSet(true);

            if (columns != null)
            {
                var columnList = columns.StringToSet().Select(i => i.Trim()).ToList();
                columnSet = new ColumnSet(columnList.ToArray());
            }

            query.EntityName = entityName;
            query.ColumnSet = columnSet;

            if (filterExpressionList != null && filterExpressionList.Any())
            {
                //add filter expression
                query.Criteria.Filters.AddRange(filterExpressionList);
            }
            if (orderExpressionList != null && orderExpressionList.Any())
            {
                foreach (var orderExpression in orderExpressionList)
                {
                    query.AddOrder(orderExpression.AttributeName, orderExpression.OrderType);
                }
            }
            if (joinExpressionList != null && joinExpressionList.Any())
            {
                foreach (var joinExpression in joinExpressionList)
                {
                    query.JoinEntity(joinExpression);
                }
            }


            var result = ConnectionHelper.EntityUtility.service.RetrieveMultiple(query).Entities;
            var dynamicModelList = new List<CrmDynamicEntityModel>();
            
            foreach (var entity in result)
            {
                var dynamicModel = entity.EntityToDynamicModel();
                dynamicModelList.Add(dynamicModel);
            }
            
            return dynamicModelList;
        }

        public static List<CrmDynamicEntityModel> GetDynamicEntityList(string entityName, string columns = null, FilterExpression filterExpression = null, OrderExpression orderExpression = null, JoinExpression joinExpression = null)
        {
            var filterExpressionList = filterExpression != null ? new List<FilterExpression> { filterExpression } : null;
            var orderExpressionList = orderExpression != null ? new List<OrderExpression> { orderExpression } : null;
            var joinExpressionList = joinExpression != null ? new List<JoinExpression> { joinExpression } : null;
            return GetDynamicEntityListAdvanced(entityName, columns, filterExpressionList, orderExpressionList, joinExpressionList);
        }

        /// <summary>
        /// Method to set value of dynamic entity field.
        /// Will add a field if it does not exist.
        /// </summary>
        /// <param name="fieldList"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="regardingEntity">only required for new records</param>
        /// <returns></returns>
        public static List<CrmDynamicEntityField> SetEntityFieldValue(this List<CrmDynamicEntityField> fieldList, string fieldName, object value, string regardingEntity = null)
        {
            var field = fieldList.FirstOrDefault(i => i.Name.Equals(fieldName));
            
            if (field != null)
            {
                field.Value = value;
                regardingEntity = field.RegardingEntity;
            }
            if (!fieldList.Any(i => i.Name.Equals(fieldName)))
            {
                //if it doesn't exist, perform an add.
                var objectType = value.GetType();
                var fieldType = GetCrmFieldType(objectType, regardingEntity);
                fieldList.Add(new CrmDynamicEntityField
                {
                    Name = fieldName,
                    Type = fieldType,
                    Value = value,
                    RegardingEntity = regardingEntity
                });
            }
            return fieldList;
        }

        /// <summary>
        /// Saves 1 entity record
        /// </summary>
        /// <param name="dynamicEntity"></param>
        /// <param name="addIds">Manually generated Guids for new records</param>
        /// <returns></returns>
        public static MessageModel SaveDynamicEntity(CrmDynamicEntityModel dynamicEntity)
        {
            var messageModel = new MessageModel();

            try
            { 
                var dataFields = dynamicEntity.FieldList.ToList();
                var fields = dataFields.Select(i => i.Name).Distinct();
                var columnSet = new ColumnSet(fields.ToArray());

                var result = dynamicEntity.ToEntity();

                if (result.EntityState == EntityState.Created)
                {
                    //add
                    var createRequest = new CreateRequest()
                    {
                        Target = result
                    };
                    var response = (CreateResponse)ConnectionHelper.EntityUtility.service.Execute(createRequest);
                    dynamicEntity.Id = response.id;    //Terence: I think this returns the ID, have not tested.
                }
                else if(result.EntityState == EntityState.Changed)
                {
                    //update
                    var updateRequest = new UpsertRequest()
                    {
                        Target = result
                    };
                    var response = (UpsertResponse)ConnectionHelper.EntityUtility.service.Execute(updateRequest);
                }
                
                messageModel.ReturnId = dynamicEntity.Id.ToString();
                messageModel.Saved = true;
                messageModel.Message = $"Saved {dynamicEntity.Name} record for ID {dynamicEntity.Id}";
                return messageModel;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"XrmPath.CRM.DataAccess caught error on CrmDynamicEntityHelper.SaveDynamicEntity()", ex);
                messageModel.Saved = false;
                messageModel.Error = true;
                messageModel.Message = ex.ToString();
                return messageModel;
            }
        }

        /// <summary>
        /// Saves a collection of entities via batch request
        /// </summary>
        /// <param name="dynamicEntityList"></param>
        /// <param name="addIds">Manually generated Guids for new records</param>
        /// <returns></returns>
        public static MessageModel SaveDynamicEntities(IEnumerable<CrmDynamicEntityModel> dynamicEntityList)
        {
            var messageModel = new MessageModel();
            var hasError = false;

            try
            {
                var entitiesToSave = new List<Entity>();
                foreach (var dynamicEntity in dynamicEntityList)
                {
                    var entityToSave = dynamicEntity.ToEntity();

                    //var allAttributes = entityToSave.Attributes.Select(i => i.Key).ToList();
                    //foreach (var dynamicField in dynamicEntity.FieldList)
                    //{
                    //    var attribute = GetAttributeValue(dynamicField);
                    //    entityToSave.Attributes[dynamicField.Name] = attribute;
                    //}

                    entitiesToSave.Add(entityToSave);
                }


                //now save all entities in batches
                if (entitiesToSave.Any())
                {
                    var organizationService = ConnectionHelper.EntityUtility.GetOrganizationService();
                    hasError = BatchHelper.BatchAddUpdate(organizationService, entitiesToSave);
                }

                if (hasError)
                {
                    messageModel.Error = true;
                    messageModel.Message = $"Error saving one ore more entities.";
                }
                else
                {
                    messageModel.Error = false;
                    messageModel.Message = $"All entities were successfully saved.";
                }

                return messageModel;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"XrmPath.CRM.DataAccess caught error on CrmDynamicEntityHelper.SaveDynamicEntities()", ex);
                messageModel.Saved = false;
                messageModel.Error = true;
                messageModel.Message = ex.ToString();
                return messageModel;
            }
        }

        public static MessageModel DeleteDynamicEntities(IEnumerable<CrmDynamicEntityModel> dynamicEntityList) 
        {
            var messageModel = new MessageModel();
            var hasError = false;
            try
            {
                var entitiesToDelete = new List<EntityReference>();
                foreach (var dynamicEntity in dynamicEntityList) 
                {
                    var entityName = dynamicEntity.Name;
                    var entityId = dynamicEntity.Id;
                    var entityReference = new EntityReference(entityName, entityId);
                    entitiesToDelete.Add(entityReference);
                }

                if (entitiesToDelete.Any()) {
                    var organizationService = ConnectionHelper.EntityUtility.GetOrganizationService();
                    hasError = BatchHelper.BatchDelete(organizationService, entitiesToDelete);
                }

                if (hasError)
                {
                    messageModel.Error = true;
                    messageModel.Message = $"Error saving one ore more entities.";
                }
                else
                {
                    messageModel.Error = false;
                    messageModel.Message = $"All entities were successfully saved.";
                }

                return messageModel;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"XrmPath.CRM.DataAccess caught error on CrmDynamicEntityHelper.DeleteDynamicEntities()", ex);
                messageModel.Error = true;
                messageModel.Message = ex.ToString();
                return messageModel;
            }
        }

        public static Entity ToEntity(this CrmDynamicEntityModel entityModel)
        {
            var entity = new Entity();

            entity.Id = entityModel.Id;
            entity.LogicalName = entityModel.Name;

            var allAttributes = entity.Attributes.Select(i => i.Key).ToList();
            foreach (var dynamicField in entityModel.FieldList)
            {
                var attribute = GetAttributeValue(dynamicField);
                entity.Attributes[dynamicField.Name] = attribute;
            }

            if (entityModel.Id == null || entityModel.Id == Guid.Empty)
            {
                entity.EntityState = EntityState.Created;
            }
            else
            {
                entity.EntityState = EntityState.Changed;
            }
            return entity;
        }

        public static MessageModel SaveEntityToCrm<T>(T dynamic)
        {
            var dynamicEntity = TransformHelper.TransformToEntity(dynamic);
            var message = SaveDynamicEntity(dynamicEntity);
            return message;
        }
        public static MessageModel SaveEntitiesToCrm<T>(List<T> dynamicList)
        {
            var dynamicEntities = dynamicList.Select(i =>  TransformHelper.TransformToEntity(i));
            var message = SaveDynamicEntities(dynamicEntities);
            return message;
        }

        public static MessageModel DeleteEntitiesFromCrm<T>(List<T> dynamicList)
        {
            var dynamicEntities = dynamicList.Select(i => TransformHelper.TransformToEntity(i));
            var message = DeleteDynamicEntities(dynamicEntities);
            return message;
        }

        public static CrmDynamicEntityField GetDynamicEntityField(KeyValuePair<string, object> attr)
        {
            var field = new CrmDynamicEntityField();
            var type = attr.Value.GetType().Name;
            var attrValue = attr.Value;
            var regardingEntity = string.Empty;
            object value = null;
            if (type == "OptionSetValue")
            {
                attrValue = ((OptionSetValue)attr.Value).Value;
                value = GetValueByType(attrValue, type, null, true);
            }
            else if (type == "EntityReference") 
            { 
                var entityRef = ((EntityReference)GetValueByType(attrValue, type));
                value = entityRef.Id;
                regardingEntity = entityRef.LogicalName;
            }
            else
            {
                value = GetValueByType(attrValue, type);
            }
            
            field.Name = attr.Key;
            field.Value = value;
            field.Type = type;
            field.RegardingEntity = regardingEntity;
            return field;
        }
        
        public static CrmDynamicEntityModel EntityToDynamicModel(this Entity entity)
        {
            if (entity == null) 
            {
                return null;
            }
            var dynamicModel = new CrmDynamicEntityModel
            {
                Id = entity.Id,
                Name = entity.LogicalName
            };

            if (entity.Attributes != null)
            {
                dynamicModel.OriginalFieldList = entity.Attributes;
            }

            foreach (var attr in entity.Attributes)
            {
                var field = GetDynamicEntityField(attr);
                if (ValidFieldTypesList.Contains(field.Type))
                {
                    dynamicModel.FieldList.Add(field);
                }
            }
            return dynamicModel;
        }

        /// <summary>
        /// This method converts a dynamic type to the correct data type.
        /// </summary>
        /// <param name="dynamic"></param>
        /// <param name="type"></param>
        /// <param name="forceString"></param>
        /// <returns></returns>
        public static object GetValueByType(object dynamic, string type, string regardingEntity = null, bool forceString = false)
        {
            var valid = true;
            object value = null;

            if (!string.IsNullOrEmpty(regardingEntity))
            {
                type = "EntityReference";
            }

            if (dynamic == null)
            {
                return null;
            }

            if (type == "String")
            {
                value = dynamic;
            }
            else if (type == "OptionSetValue")
            {
                if (dynamic.GetType() != typeof(OptionSetValue))
                {
                    var optionSet = new OptionSetValue(int.Parse(dynamic.ToString()));
                    if (forceString)
                    {
                        value = optionSet.Value.ToString();
                    }
                    else
                    {
                        value = optionSet;
                    }
                }
                else
                {
                    value = ((OptionSetValue)dynamic);
                }

            }
            else if (type == "Guid")
            {
                if (dynamic.GetType() != typeof(Guid))
                {
                    value = Guid.Parse(dynamic.ToString());
                }
                else
                {
                    value = ((Guid)dynamic);
                }    
            }
            else if (type == "Double")
            {
                if (dynamic.GetType() != typeof(double))
                {
                    double doubleValue = 0;
                    valid = double.TryParse(dynamic.ToString(), out doubleValue);
                    if (valid)
                    {
                        value = doubleValue;
                    }
                }
                else
                {
                    value = ((double)dynamic);
                }
            }
            else if (type == "Decimal")
            {
                if (dynamic.GetType() != typeof(decimal))
                {
                    decimal decimalValue = 0;
                    valid = decimal.TryParse(dynamic.ToString(), out decimalValue);
                    if (valid)
                    {
                        value = decimalValue;
                    }
                }
                else
                {
                    value = ((decimal)dynamic);
                }
            }
            else if (type == "Boolean")
            {
                if (dynamic.GetType() != typeof(bool))
                {

                    var booleanValue = StringUtility.ToBoolean(dynamic.ToString());
                    value = booleanValue;
                }
                else
                {
                    value = ((bool)dynamic);
                }
            }
            else if (type == "Currency" || type == "Money")
            {
                decimal decimalValue = 0;
                if (dynamic.GetType() != typeof(Money))
                {
                    valid = decimal.TryParse(dynamic.ToString(), out decimalValue);
                    if (valid)
                    {
                        value = new Money(decimalValue);
                    }
                }
                else
                {
                    value = ((Money)dynamic);
                }
            }
            else if (type == "Int32")
            {
                if (dynamic.GetType() != typeof(int))
                {
                    var intValue = 0;
                    valid = int.TryParse(dynamic.ToString(), out intValue);
                    if (valid)
                    {
                        value = intValue;
                    }
                }
                else {
                    value = ((int)dynamic);
                }
            } 
            else if (type == "DateTime")
            {
                if (dynamic.GetType() != typeof(DateTime))
                {
                    var dateTime = DateTime.MinValue;
                    valid = DateTime.TryParse(dynamic.ToString(), out dateTime);
                    if (valid)
                    {
                        value = dateTime;
                    }
                }
                else
                {
                    value = ((DateTime)dynamic);
                }
            }
            else if (type == "EntityReference") 
            {
                if (dynamic.GetType() == typeof(EntityReference))
                {
                    var entityRef = ((EntityReference)dynamic);
                    value = entityRef;
                }
                else if (!string.IsNullOrEmpty(regardingEntity))
                {
                    //need to change this data type to type EntityReference
                    var validGuid = Guid.TryParse(dynamic.ToString(), out Guid entityId);
                    if (validGuid && entityId != Guid.Empty)
                    {
                        var entityRef = new EntityReference(regardingEntity, entityId);
                        value = entityRef;
                    }
                }
            }

            return value;
        }

        public static string GetCrmFieldType(Type fieldType, string regardingEntity = null)
        {
            var type = "String";
            if (fieldType == typeof(string) || fieldType == typeof(String))
            {
                type = "String";
            }
            else if (fieldType == typeof(Guid) || fieldType == typeof(Guid?))
            {
                type = "Guid";
            }
            if (fieldType == typeof(bool) || fieldType == typeof(bool?))
            {
                type = "Boolean";
            }
            if (fieldType == typeof(double?) || fieldType == typeof(double?))
            {
                type = "Double";
            }
            if (fieldType == typeof(decimal) || fieldType == typeof(decimal?))
            {
                type = "Decimal";
            }
            if (fieldType == typeof(Money))
            {
                type = "Currency";
            }
            if (fieldType == typeof(int?) || fieldType == typeof(int) || fieldType == typeof(Int32))
            {
                type = "Int32";
            }
            if (fieldType == typeof(DateTime) || fieldType == typeof(DateTime?))
            {
                type = "DateTime";
            }
            if (!string.IsNullOrEmpty(regardingEntity) || fieldType == typeof(EntityReference))
            {
                type = "EntityReference";
            }
            return type;
        }

        private static object GetAttributeValue(CrmDynamicEntityField dynamicField)
        {
            var value = GetValueByType(dynamicField.Value, dynamicField.Type, dynamicField.RegardingEntity);
            return value;
        }

        public static T GetAttributeByKey<T>(this AttributeCollection attributeCollection, string key)
        {
            if (!attributeCollection.ContainsKey(key)) {
                return default;    
            }
            var keyValuePair = attributeCollection[key];
            var type = GetCrmFieldType(typeof(T));
            var attribute = GetValueByType(keyValuePair, type, null, false);
            return (T)attribute;
        }

        public static FilterExpression AddConditionExpression(this FilterExpression filterExpression, string attributeName, ConditionOperator conditionOperator, params object[] values)
        {
            //do not use this method to query multiple ids. issue with params object[] values
            //juse call "AddCondition" without running it through this method
            if (filterExpression == null)
            {
                filterExpression = new FilterExpression();
            }
            filterExpression.AddCondition(attributeName, conditionOperator, values);
            return filterExpression;
        }
        public static List<OrderExpression> AddOrderExpression(this List<OrderExpression> orderExpressionList, string attributeName, OrderType orderType)
        {
            if (orderExpressionList == null)
            {
                orderExpressionList = new List<OrderExpression>();
            }
            orderExpressionList.Add(new OrderExpression { AttributeName = attributeName, OrderType = orderType });
            return orderExpressionList;
        }

        public static LinkEntity JoinEntity(this QueryExpression qx, JoinExpression joinExpression)
        {
            LinkEntity linkEntity = qx.AddLink(joinExpression.LinkToEntityName, joinExpression.LinkFromAttributeName, joinExpression.LinkToAttributeName, joinExpression.JoinOperator);
            var columnList = joinExpression.Columns != null ? joinExpression.Columns.Split(',').ToList() : new List<string>();
            foreach (var column in columnList)
            {
                linkEntity.Columns.AddColumn(column);
            }
            linkEntity.EntityAlias = joinExpression.LinkToEntityName;
            return linkEntity;
        }

        public static List<CrmDynamicEntityModel> DetectDuplicates(this IOrganizationService service, CrmDynamicEntityModel dynamicModel)
        {

            var dynamicModelList = new List<CrmDynamicEntityModel>();

            try
            {
                var entity = dynamicModel.ToEntity();

                var request = new RetrieveDuplicatesRequest
                {
                    //Entity Object to be searched with the values filled for the attributes to check
                    BusinessEntity = entity,

                    //Logical Name of the Entity to check Matching Entity
                    MatchingEntityName = entity.LogicalName,
                    PagingInfo = new PagingInfo() { PageNumber = 1, Count = 50 }

                };
                var response = (RetrieveDuplicatesResponse)service.Execute(request);
                var entityList = new List<Entity>();
                if (response.DuplicateCollection.Entities.Any())
                {
                    entityList = response.DuplicateCollection.Entities.ToList();
                }
                dynamicModelList = entityList.Select(i => i.EntityToDynamicModel()).ToList();
            }
            catch (Exception ex)
            {
                //LogHelper.Error($"PPE.Helpers caught error on CrmDynamicEntityHelper.DetectDuplicates()", ex);
                throw ex;
            }

            return dynamicModelList;
        }

    }
}

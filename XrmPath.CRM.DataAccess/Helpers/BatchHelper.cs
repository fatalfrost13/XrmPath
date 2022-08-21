using XrmPath.CRM.DataAccess.Models;
using log4net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmPath.CRM.DataAccess.Helpers
{

    /// <summary>
    /// Note that the maximum bulk size for requesting to execute multiple responses is 1000
    /// If number exceeds 1000, we need to separate these into multiple bulk requests.
    /// </summary>
    public static class BatchHelper
    {

        public static ILog LogHelper = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly int MaxBatchSize = 1000;

        /// <summary>
        /// Can pass in a combination of add/update requests
        /// Ensure that you set the EntityState accordingly
        /// Returns boolean indicating if there's an error
        /// </summary>
        /// <param name="service"></param>
        /// <param name="updatedEntities"></param>
        public static bool BatchAddUpdate(IOrganizationService service, List<Entity> allEntities)
        {
            var batchList = GetBatchList(allEntities);
            var hasError = false;
            foreach (var batch in batchList)
            {
                var entities = batch.EntityList;
                // Create an ExecuteMultipleRequest object.
                var multipleRequest = new ExecuteMultipleRequest()
                {
                    // Assign settings that define execution behavior: continue on error, return responses. 
                    Settings = new ExecuteMultipleSettings()
                    {
                        ContinueOnError = false,
                        ReturnResponses = true
                    },
                    // Create an empty organization request collection.
                    Requests = new OrganizationRequestCollection()
                };

                var executeUpdate = false;
                // Add a UpdateRequest for each entity to the request collection.
                foreach (var entity in entities)
                {
                    if (entity.EntityState == null)
                    {
                        entity.EntityState = EntityState.Unchanged;

                        //need to determine if it's an add or update.
                        if (entity.Id == null || entity.Id == Guid.Empty)
                        {
                            entity.EntityState = EntityState.Created;
                        }
                        else
                        {
                            entity.EntityState = EntityState.Changed;
                        }
                    }
                    if (entity.EntityState == EntityState.Created)
                    {
                        CreateRequest createRequest = new CreateRequest { Target = entity };
                        multipleRequest.Requests.Add(createRequest);
                        executeUpdate = true;
                    }
                    else if(entity.EntityState == EntityState.Changed)
                    {
                        UpdateRequest updateRequest = new UpdateRequest { Target = entity };
                        multipleRequest.Requests.Add(updateRequest);
                        executeUpdate = true;
                    }
                }

                if (executeUpdate)
                {
                    // Execute all the requests in the request collection using a single web method call.
                    ExecuteMultipleResponse multipleResponse = (ExecuteMultipleResponse)service.Execute(multipleRequest);
                    hasError = multipleResponse.IsFaulted;
                    if (hasError)
                    {
                        var errorList = multipleResponse.Responses.Where(i => i.Fault != null).Select(i => i.Fault.Message).ToList();
                        LogHelper.Error($"XrmPath.CRM.DataAccess caught error on BatchHelper.BatchAddUpdate()", new Exception(string.Join("\n", errorList)));
                    }
                    
                }
            }
            return hasError;
        }

        /// <summary>
        /// Call this method for bulk delete
        /// </summary>
        /// <param name="service">Org Service</param>
        /// <param name="entityReferences">Collection of EntityReferences to Delete</param>
        public static bool BatchDelete(IOrganizationService service, List<EntityReference> entityReferences)
        {
            var hasError = false;
            // Create an ExecuteMultipleRequest object.
            var multipleRequest = new ExecuteMultipleRequest()
            {
                // Assign settings that define execution behavior: continue on error, return responses. 
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = false,
                    ReturnResponses = true
                },
                // Create an empty organization request collection.
                Requests = new OrganizationRequestCollection()
            };

            var executeDelete = false;
            // Add a DeleteRequest for each entity to the request collection.
            foreach (var entityRef in entityReferences)
            {
                DeleteRequest deleteRequest = new DeleteRequest { Target = entityRef };
                multipleRequest.Requests.Add(deleteRequest);
                executeDelete = true;
            }

            // Execute all the requests in the request collection using a single web method call.
            if (executeDelete) 
            {
                ExecuteMultipleResponse multipleResponse = (ExecuteMultipleResponse)service.Execute(multipleRequest);
                hasError = multipleResponse.IsFaulted;
            }
           
            return hasError;

        }

        /// <summary>
        /// Splits a list of entities into batches of a specified size (MaxBatchSize).
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private static List<BatchModel> GetBatchList(List<Entity> entities)
        {
            var batchList = new List<BatchModel>();
            if (entities.Count < MaxBatchSize)
            {
                var batch = new BatchModel { EntityList = entities };
                batchList.Add(batch);
            }
            else
            {
                var index = 0;
                //larger than 1000 records. needs to be split
                batchList = entities.GroupBy(i => (index++) + 1 / MaxBatchSize).Select(i => new BatchModel { EntityList = i.Select(x => x).ToList() } ).ToList();
            }
            return batchList;
        }
    }
}

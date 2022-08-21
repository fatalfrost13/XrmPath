using System;
using System.Collections.Generic;
using System.Web.Services.Protocols;
using XrmPath.CRM.DataAccess.Base;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace XrmPath.CRM.DataAccess.Helpers.Utilities
{
    public class CrmUtility: CrmBase
    {

        public bool DeAssociateManyToManyEntityRecords(Entity moniker1, Entity moniker2, string entityRelationshipName)
        {
            var success = false;
            try
            {
                var relationshipExists = RelationshipExists(moniker1, moniker2, entityRelationshipName);
                if (relationshipExists)
                { 
                    // Create an AssociateEntities request.
                    var request = new DisassociateEntitiesRequest
                    {
                        Moniker1 = new EntityReference {Id = moniker1.Id, LogicalName = moniker1.LogicalName},  // Set the ID of Moniker1 to the ID of the lead.
                        Moniker2 = new EntityReference {Id = moniker2.Id, LogicalName = moniker2.LogicalName},  // Set the ID of Moniker2 to the ID of the contact.
                        RelationshipName = entityRelationshipName    // Set the relationship name to associate on.
                    };
                    // Execute the request.
                    service.Execute(request);
                }
                success = true;
            }
            catch (SoapException ex)
            {
                Log.Error($"XrmPath.CRM.DataAccess caught error on CrmUtility.DeAssociateManyToManyEntityRecords.", ex);
            }
            return success;
        }

        public bool AssociateManyToManyEntityRecords(Entity moniker1, Entity moniker2, string entityRelationshipName)
        {
            var success = false;
            try
            {
                var relationshipExists = RelationshipExists(moniker1, moniker2, entityRelationshipName);
                if (!relationshipExists)
                { 
                    var request = new AssociateEntitiesRequest
                    {
                        Moniker1 = new EntityReference {Id = moniker1.Id, LogicalName = moniker1.LogicalName},
                        Moniker2 = new EntityReference {Id = moniker2.Id, LogicalName = moniker2.LogicalName},
                        RelationshipName = entityRelationshipName
                    };
                    service.Execute(request);
                }
                success = true;
            }
            catch (SoapException ex)
            {
                Log.Error($"XrmPath.CRM.DataAccess caught error on CrmUtility.AssociateManyToManyEntityRecords.", ex);
            }

            return success;
        }

        public bool DisassociateMembersFromCRMList(List<Guid> memberIds, Guid listId)
        {
            var success = false;
            try
            {
                var removeRequest = new RemoveMemberListRequest();
                foreach (var memberId in memberIds)
                {
                    removeRequest.EntityId = memberId;
                    removeRequest.ListId = listId;
                    service.Execute(removeRequest);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"XrmPath.CRM.DataAccess caught error on CrmUtility.DisassociateMembersFromCRMList.", ex);
            }

            return success;
        }

        public bool AssociateMembersToCRMList(List<Guid> memberIds, Guid listId)
        {
            var success = false;
            try
            {
                var addRequest = new AddMemberListRequest();
                foreach (var memberId in memberIds)
                {
                    addRequest.EntityId = memberId;
                    addRequest.ListId = listId;
                    service.Execute(addRequest);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"XrmPath.CRM.DataAccess caught error on CrmUtility.AssociateMembersToCRMList.", ex);
            }
            return success;
        }

        /// <summary>
        /// Test to see if this works
        /// </summary>
        /// <param name="moniker1"></param>
        /// <param name="moniker2"></param>
        /// <param name="entityRelationshipName"></param>
        /// <returns></returns>
        private bool RelationshipExists(Entity moniker1, Entity moniker2, string entityRelationshipName)
        {
            string relationship1EtityName = string.Format("{0}id", moniker1.LogicalName);
            string relationship2EntityName = string.Format("{0}id", moniker2.LogicalName);

            //This check is added for self-referenced relationships
            if (moniker1.LogicalName.Equals(moniker2.LogicalName, StringComparison.InvariantCultureIgnoreCase))
            {
                relationship1EtityName = string.Format("{0}idone", moniker1.LogicalName);
                relationship1EtityName = string.Format("{0}idtwo", moniker1.LogicalName);
            }

            QueryExpression query = new QueryExpression(moniker1.LogicalName) { ColumnSet = new ColumnSet(false) };

            LinkEntity link = query.AddLink(entityRelationshipName, 
            string.Format("{0}id", moniker1.LogicalName), relationship1EtityName);

            link.LinkCriteria.AddCondition(relationship1EtityName, 
            ConditionOperator.Equal, new object[] { moniker1.Id });

            link.LinkCriteria.AddCondition(relationship2EntityName, 
            ConditionOperator.Equal, new object[] { moniker2.Id });

            return service.RetrieveMultiple(query).Entities.Count != 0;
        }
    }
}
using XrmPath.CRM.DataAccess.Helpers.Utilities;
using XrmPath.CRM.DataAccess.Model;
using XrmPath.CRM.DataAccess.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;

namespace XrmPath.CRM.DataAccess.Helpers
{
    
    public static class SampleHelper
    {

        //public static EntityUtility EntityUtility = ConnectionHelper.EntityUtility;     //entity utility for pulling data only
        //public static CrmUtility CrmUtility = ConnectionHelper.CrmUtility;

        /// <summary>
        /// This is just a sample of getting a record from CRM
        /// </summary>
        /// <param name="email"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public static List<Contact> SampleContactList(string email, CrmDataContext dataContext = null)
        {
    
            if (dataContext == null)
            {
                dataContext = ConnectionHelper.EntityUtility.DataContext;    //use global context is null passed it for data pull.
            }
            var contact = dataContext.ContactSet.Where(i => (int)i.StatusCode == (int)contact_statuscode.Active).Where(i => i.EMailAddress1 == email).ToList();

            //you can return contact or transform it into a view model
            return contact;
        }

        /// <summary>
        /// This is just a sample of posting a record to CRM
        /// </summary>
        /// <param name="email"></param>
        /// <param name="dataContext"></param>
        public static void SamplePostContact(string email, CrmDataContext dataContext = null)
        {

            if (dataContext == null)
            {
                dataContext = ConnectionHelper.CrmUtility.GetDataContext();  //get new context if null datacontext passed in.
            }

            var contact = SampleContactList(email, dataContext).FirstOrDefault();
            contact.JobTitle = "Web Developer";

            if (!dataContext.IsAttached(contact))
            {
                dataContext.Attach(contact);
            }

            dataContext.UpdateObject(contact);
            var saved = ConnectionHelper.EntityUtility.SaveCrmChanges(dataContext);  //return if value has been saved.
        }

        public static List<CrmDynamicEntityModel> SampleBulkAddUpdate(string email, CrmDataContext dataContext = null)
        {
            if (dataContext == null)
            {
                dataContext = ConnectionHelper.CrmUtility.GetDataContext();  //get new context if null datacontext passed in.
            }
            var contactList = SampleContactList(email);

            //After pulling a couple test contact records, populate custom dynamic entity object.
            //Then update the job title field to whatever we want to test bulk upload.
            //Then run bulk upload with only updated fields.
            /***** Everything is hardcoded here just to demonstrate updating 2 records *****/
            var fieldToUpdate = "jobtitle";
            var dynamicEntityCollection = new List<CrmDynamicEntityModel>();
            var index = 1;
            foreach (var contact in contactList)
            {
                var positionData = $"Web Developer {index}";
                var entityModel = new CrmDynamicEntityModel
                {
                    Id = contact.Id,
                    Name = contact.LogicalName,
                    FieldList = new List<CrmDynamicEntityField> {
                        new CrmDynamicEntityField {
                            Name = fieldToUpdate,
                            Type = "String",
                            Value = positionData
                        }
                    }
                };
                dynamicEntityCollection.Add(entityModel);
                index++;
            }
            /***** End of hardcoded code *****/

            var hasError = CrmDynamicEntityHelper.SaveDynamicEntities(dynamicEntityCollection);

            return dynamicEntityCollection;
        }

        public static string GetProgramSpecializations()
        {
            var test = CrmDynamicEntityHelper.GetDynamicEntity("new_programspecialization", new Guid("2f7788f8-6efa-e911-911a-005056944544"));
            return null;
        }

        //public static List<AccountModel> GetAccountTest1()
        //{

        //    var context = ConnectionHelper.EntityUtility.DataContext;

        //    //list of account ids
        //    var accountIds = new List<Guid>();

        //    //this will query out and join accounts to incidents
        //    //this object contains incidents with account information attached to it.
        //    var accountIncidents = (from a in context.AccountSet
        //                            join i in context.IncidentSet on a.AccountId.Value equals i.AccountId.Id into ai
        //                            from i in ai.DefaultIfEmpty()
        //                            select new
        //                            {
        //                                a,
        //                                i
        //                            }).ToList()
        //                  .Where(x => !accountIds.Any() || accountIds.Contains((Guid)x.a.AccountId)).ToList();

        //    //now we need to transform it into a more friendly view
        //    //a list of accounts with sub list of incidents
        //    var accountList = new List<AccountModel>();
        //    foreach (var incident in accountIncidents)
        //    {

        //        var findAccount = accountList.SingleOrDefault(i => i.Id == incident.a.AccountId);
        //        if (findAccount == null)
        //        {
        //            var newAccount = new AccountModel
        //            {
        //                Id = (Guid)incident.a.AccountId,
        //                Name = incident.a.Name
        //            };

        //            findAccount = newAccount;
        //            accountList.Add(findAccount);
        //        }

        //        if (incident.i?.IncidentId != null && incident.i?.IncidentId != Guid.Empty)
        //        {
        //            if (findAccount != null)
        //            {
        //                var newIncident = new IncidentModel
        //                {
        //                    Id = (Guid)incident.i.IncidentId,
        //                    TicketNumber = incident.i?.TicketNumber ?? string.Empty
        //                };
        //                findAccount.IncidentList.Add(newIncident);
        //            }
        //        }
        //    }
        //    return accountList;
        //}

        //public static List<AccountModel> GetAccountTest2()
        //{
        //    var accountList = new List<AccountModel>();

        //    //list of account ids
        //    var accountIds = new List<Guid>();

        //    QueryExpression qx = new QueryExpression("account");
        //    qx.ColumnSet.AddColumn("accountid");
        //    qx.ColumnSet.AddColumn("name");

        //    LinkEntity link = qx.AddLink("incident", "accountid", "customerid", JoinOperator.LeftOuter);
        //    link.Columns.AddColumn("incidentid");
        //    link.Columns.AddColumn("ticketnumber");
        //    link.EntityAlias = "incident";

        //    //use this to pull only accounts with incidents
        //    //qx.Criteria = new FilterExpression();
        //    //qx.Criteria.AddCondition("incident", "incidentid", ConditionOperator.NotNull);

        //    //if there are any accountids in the list, filter by account ids
        //    if (accountIds.Any())
        //    {
        //        var condition = new ConditionExpression("account", "accountid", ConditionOperator.In, accountIds.ToArray());
        //        qx.Criteria = new FilterExpression();
        //        qx.Criteria.AddCondition(condition);
        //    }

        //    var result = ConnectionHelper.EntityUtility.service.RetrieveMultiple(qx).Entities;

        //    foreach (var incident in result)
        //    {
        //        var accountId = (Guid)incident.Attributes["accountid"];
        //        var accountName = (string)incident.Attributes["name"];
        //        var incidentId = incident.Attributes.ContainsKey("incident.incidentid") ? (Guid)((AliasedValue)incident.Attributes["incident.incidentid"]).Value : (Guid?)null;
        //        var ticketNumber = incident.Attributes.ContainsKey("incident.ticketnumber") ? (string)((AliasedValue)incident.Attributes["incident.ticketnumber"]).Value : null;

        //        var findAccount = accountList.SingleOrDefault(i => i.Id == accountId);
        //        if (findAccount == null)
        //        {
        //            var newAccount = new AccountModel
        //            {
        //                Id = accountId,
        //                Name = accountName
        //            };

        //            findAccount = newAccount;
        //            accountList.Add(findAccount);
        //        }

        //        //due to left outer join, we may have null incident ids
        //        if (findAccount != null && incidentId != null)
        //        {
        //            var newIncident = new IncidentModel
        //            {
        //                Id = (Guid)incidentId,
        //                TicketNumber = ticketNumber
        //            };
        //            findAccount.IncidentList.Add(newIncident);
        //        }
        //    }

        //    return accountList;
        //}

        //public static List<AccountModel> GetAccountTest3()
        //{
        //    var accountList = new List<AccountModel>();

        //    //list of account ids
        //    var accountIds = new List<Guid>();

        //    var joinExpression = new JoinExpression
        //    {
        //        LinkToEntityName = "incident",
        //        LinkFromAttributeName = "accountid",
        //        LinkToAttributeName = "customerid",
        //        JoinOperator = JoinOperator.LeftOuter,
        //        Columns = "incidentid,ticketnumber"
        //    };

        //    FilterExpression filterExpression = null;
        //    //if there are any accountids in the list, filter by account ids
        //    if (accountIds.Any())
        //    {
        //        var condition = new ConditionExpression("account", "accountid", ConditionOperator.In, accountIds.ToArray());
        //        filterExpression.AddCondition(condition);
        //    }

        //    var result = CrmDynamicEntityHelper.GetDynamicEntityList("account", "accountid,name", filterExpression, null, joinExpression);

        //    foreach (var incident in result)
        //    {
        //        var accountId = incident.OriginalFieldList.GetAttributeByKey<Guid>("accountid");
        //        var accountName = incident.OriginalFieldList.GetAttributeByKey<string>("name");
        //        var incidentId = (Guid?)incident.OriginalFieldList.GetAttributeByKey<AliasedValue>("incident.incidentid")?.Value ?? null;
        //        var ticketNumber = (string)incident.OriginalFieldList.GetAttributeByKey<AliasedValue>("incident.ticketnumber").Value;

        //        var findAccount = accountList.SingleOrDefault(i => i.Id == accountId);
        //        if (findAccount == null)
        //        {
        //            var newAccount = new AccountModel
        //            {
        //                Id = accountId,
        //                Name = accountName
        //            };

        //            findAccount = newAccount;
        //            accountList.Add(findAccount);
        //        }

        //        //due to left outer join, we may have null incident ids
        //        if (findAccount != null && incidentId != null)
        //        {
        //            var newIncident = new IncidentModel
        //            {
        //                Id = (Guid)incidentId
        //                //TicketNumber = ticketNumber
        //            };
        //            // findAccount.IncidentList.Add(newIncident);
        //        }
        //    }
        //    return accountList;
        //}


        /// <summary>
        /// Sample adding task with reference to case
        /// </summary>
        /// <returns></returns>
        public static MessageModel GetTestTask()
        {
            var taskModel = new TaskModel
            {
                Id = Guid.Empty,
                Name = "Test Task Subject",
                Description = "Test Description",
                RegardingObjectIdCase = Guid.Parse("be9858f0-abf9-e911-911a-005056944544"),
            };
            var message = CrmDynamicEntityHelper.SaveEntityToCrm(taskModel);
            return message;
        }

        /// <summary>
        /// Gets Dynamic Entity and it's fields
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="columns"></param>
        /// <param name="top"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public static List<CrmDynamicEntityModel> GetEntityItems(string entityName, string columns = null, int? top = null, CrmDataContext dataContext = null)
        {
            if (dataContext == null)
            {
                dataContext = ConnectionHelper.EntityUtility.DataContext;    //use global context is null passed it for data pull.
            }
            if (string.IsNullOrEmpty(columns) || columns == "null")
            {
                columns = null;
            }
            var service = ConnectionHelper.CrmUtility.service;
            var list = CrmDynamicEntityHelper.GetDynamicEntityList(entityName, columns, null).ToList();
            if (top != null)
            {
                list = list.Take((int)top).ToList();
            }

            foreach (var listItem in list)
            {
                listItem.OriginalFieldList = null;
            }
            return list;
        }

    }
}

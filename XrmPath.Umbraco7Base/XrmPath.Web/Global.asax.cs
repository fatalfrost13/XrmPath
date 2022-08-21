using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Helpers;
using Umbraco.Core;
using System.Web.Http;
using System.Web.Routing;

using Examine;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using UmbracoExamine;


namespace XrmPath.Web.UmbracoApplication
{
    using System;
    using Umbraco.Core.Services;
    using Umbraco.Core.Events;
    using System.Net.Http.Formatting;
    using XrmPath.Web.Handlers;
    using XrmPath.Web.Helpers.UmbracoHelpers;
    using XrmPath.Web.Models;
    using XrmPath.Helpers.Utilities;

    public class AppStart : IApplicationEventHandler
    {

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //throw new System.NotImplementedException();
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            InitializeApiConfiguration();   
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //throw new System.NotImplementedException();
            InitializeConfiguration();
            InitializeUmbracoEvents();
        }

        protected void OnApplicationExit(object sender, EventArgs e)
        {
        }

        private static void InitializeApiConfiguration()
        {
            GlobalConfiguration.Configuration.MessageHandlers.Add(new WebApiHandler());
            GlobalConfiguration.Configuration.MessageHandlers.Add(new CompressHandler());
        }

        private static void InitializeConfiguration()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;  // Set security protocol to TLS 1.2 for version 9.0 of Customer Engagement Platform
            RegisterRoutes(RouteTable.Routes);
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("json", "true", "application/json"));
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            //configure OAuth Endpoints
            //OAuthHelper.ConfigureInvestAlbertaBasicEndpoint();
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            //map route for webapi
            RouteTable.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}/{id}", defaults: new
            {
                id = RouteParameter.Optional
            });
        }

        #region Umbraco Event Handlers
        private ISet<string> WebsiteContentTypes = ConfigurationModel.WebsiteContentTypes.StringToSet();

        private void InitializeUmbracoEvents()
        {
            ContentService.Published += ContentService_Published;
            ContentService.Saving += ContentService_OnSaving;
            ContentService.SendingToPublish += ContentService_SendingToPublish;
            ContentService.Trashing += ContentService_Trashing;
            ContentService.Moving += ContentService_Moving;
            ContentService.Publishing += ContentService_Publishing;
            CustomInitialize();
        }

        private void ContentService_Publishing(Umbraco.Core.Publishing.IPublishingStrategy sender, PublishEventArgs<IContent> publishingEventArgs)
        {
            try
            {
                foreach (var e in publishingEventArgs.PublishedEntities)
                {
                    if (e.Id > 0 && e.Published && WebsiteContentTypes.Contains(e.ContentType.Alias))
                    {
                        var content = ServiceUtility.UmbracoHelper.GetById(e.Id);
                        if (content.NodeExists())
                        {
                           
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>("XrmPath.Web caught error on Global.asax.cs ContentService_Trashing()", ex);
            }
        }

        private void ContentService_Moving(IContentService sender, MoveEventArgs<IContent> movingEventArgs)
        {
            try
            {
                
                foreach(var e in movingEventArgs.MoveInfoCollection.Where(i => WebsiteContentTypes.Contains(i.Entity.ContentType.Alias)))
                {
                    //only loop through valid website content types
                    if (e.Entity.Published && e.Entity.Id > 0)
                    {
                        var content = ServiceUtility.UmbracoHelper.GetById(e.Entity.Id);
                        if (content.NodeExists())
                        {
                           
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>("XrmPath.Web caught error on Global.asax.cs ContentService_Moving()", ex);
            }
        }

        private void ContentService_Trashing(IContentService sender, MoveEventArgs<IContent> trashEventArgs)
        {
            try
            {
                foreach (var e in trashEventArgs.MoveInfoCollection)
                {
                   
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>("XrmPath.Web caught error on Global.asax.cs ContentService_Trashing()", ex);
            }
        }

        private void ContentService_SendingToPublish(IContentService sender, SendToPublishEventArgs<IContent> sendToPublishEventArgs)
        {
            var e = sendToPublishEventArgs.Entity;
            if (e.Id > 0)
            {
                //e.ReleaseDate = DateTime.Now.AddHours(24);
                //sender.Save(e);
            }
        }

        private void ContentService_OnSaving(IContentService sender, SaveEventArgs<IContent> saveEventArgs)
        {
            //throw new NotImplementedException();
            foreach (var e in saveEventArgs.SavedEntities)
            {
                
                if (e.Id == 0)
                {
                    try
                    {
                        //ensure records have a valid title
                        var hasProperty = e.HasProperty("title");
                        var titleField = hasProperty ? (e.GetValue("title") ?? "") : "";

                        if (hasProperty && string.IsNullOrEmpty(titleField.ToString()))
                        {
                            e.SetValue("title", e.Name);
                            sender.Save(e);
                        }

                        //meta description defaults
                        var title = e.HasProperty(UmbracoCustomFields.Title) ? (e.GetValue(UmbracoCustomFields.Title)?.ToString() ?? "") : "";
                        var description = e.HasProperty(UmbracoCustomFields.Description) ? (e.GetValue(UmbracoCustomFields.Description)?.ToString() ?? "") : "";
                        var metaDescription = e.HasProperty(UmbracoCustomFields.MetaDescription) ? (e.GetValue(UmbracoCustomFields.MetaDescription)?.ToString() ?? "") : "";
                        var metaKeywords = e.HasProperty(UmbracoCustomFields.MetaKeywords) ? (e.GetValue(UmbracoCustomFields.MetaKeywords)?.ToString() ?? "") : "";
                        var dateNow = DateTime.Now.ToString("MMMM dd, yyyy");

                        if (metaDescription.IndexOf(dateNow, StringComparison.Ordinal) == -1 && !string.IsNullOrEmpty(description))
                        {
                            if (e.HasProperty(UmbracoCustomFields.MetaDescription))
                            {
                                e.SetValue(UmbracoCustomFields.MetaDescription, dateNow + " - " + description);
                            }
                        }

                        if (metaKeywords.IndexOf(title, StringComparison.Ordinal) == -1)
                        {
                            if (e.HasProperty(UmbracoCustomFields.MetaKeywords))
                            {
                                e.SetValue(UmbracoCustomFields.MetaKeywords, metaKeywords + (!string.IsNullOrEmpty(metaKeywords) ? "," : "") + title);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<string>("XrmPath.Web caught error on Global.asax.cs ContentService_OnSaving()", ex);
                    }
                }
            }
        }

        private void ContentService_Published(Umbraco.Core.Publishing.IPublishingStrategy sender, PublishEventArgs<Umbraco.Core.Models.IContent> e)
        {

        }
        private void ExamineEvents_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {
            if (e.IndexType != IndexTypes.Content) return;

            try
            {
                // Node picker values are stored as csv which will not be indexed properly 
                // We need to write the values back into the index without commas so they are indexed correctly
                var fields = e.Fields;
                var documentTypeAlias = fields["nodeTypeAlias"];

                var searchableFields = new Dictionary<string, string>();
                var uniqueId = Guid.NewGuid().ToString();
                var tagFields = ConfigurationModel.TagFields.StringToSet();

                foreach (var field in fields)
                {
                    if (tagFields.Contains(field.Key))
                    {
                        var searchableFieldKey = $"{field.Key}{uniqueId}";
                        var searchableFieldValue = field.Value.Replace(",", ", ");
                        if (!string.IsNullOrEmpty(searchableFieldValue))
                        {
                            searchableFields.Add(searchableFieldKey, searchableFieldValue);
                        }
                    }
                }

                foreach (var fld in searchableFields)
                {
                    var originalKey = fld.Key.Replace(uniqueId, "");
                    if (tagFields.Contains(originalKey))
                    {
                        e.Fields.Remove(originalKey);
                        e.Fields.Add(originalKey, fld.Value);
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error<string>("XrmPath.Web caught error on Global.asax.cs ExamineEvents_GatheringNodeData()", ex);
            }
        }
        private void UmbracoIndexer_DocumentWriting(object sender, Examine.LuceneEngine.DocumentWritingEventArgs e)
        {
            //BoostImportantFields(e);
        }
        #endregion


        private void CustomInitialize()
        {
            //custom indexing alterations
            var indexer = ExamineManager.Instance.IndexProviderCollection["ExternalIndexer"];
            var umbracoIndexer = (UmbracoContentIndexer)indexer;
            indexer.GatheringNodeData += ExamineEvents_GatheringNodeData;
            umbracoIndexer.DocumentWriting += UmbracoIndexer_DocumentWriting;
        }

        //private void BoostImportantFields(Examine.LuceneEngine.DocumentWritingEventArgs e)
        //{
        //    //use this method to boost certain fields.
        //    var fieldList = e.Fields.Select(i => i.Key).ToList();
        //    var boostedFieldsLookup = LookupHelper.BoostLookup;

        //    if (fieldList.Any() && boostedFieldsLookup.Any())
        //    {
        //        try
        //        {
        //            var commonFields = fieldList.Intersect(boostedFieldsLookup.Select(i => i.Key));
        //            var boostedFields = boostedFieldsLookup.Where(i => commonFields.Contains(i.Key));
        //            foreach (var boostedField in boostedFields)
        //            {
        //                var field = e.Document.GetField(boostedField.Key);
        //                var boostValue = (float)boostedField.ValueNumeric;
        //                field.SetBoost(boostValue);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogHelper.Error<GenericModel>("XrmPath.Web caught error on Global.asax.cs UmbracoIndexer_DocumentWriting()", ex);
        //        }
        //    }
        //}

    }
}
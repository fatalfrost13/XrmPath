using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Routing;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services.Implement;

namespace XrmPath.UmbracoCore.Handlers
{
    public class RegisterCustomRouteComposer : ComponentComposer<RegisterCustomRouteComponent>
    { }

    public class RegisterCustomRouteComponent : IComponent
    {
        public void Initialize()
        {
            // Custom route to MyProductController which will use a node with a specific ID as the
            // IPublishedContent for the current rendering page
            InitializeApiConfiguration();
            InitializeConfiguration();
            InitializeUmbracoEvents();
        }

        private static void InitializeApiConfiguration()
        {
            GlobalConfiguration.Configuration.MessageHandlers.Add(new WebApiHandler());
            GlobalConfiguration.Configuration.MessageHandlers.Add(new CompressHandler());
        }

        public void Terminate()
        {
            // Nothing to terminate
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

        private void InitializeUmbracoEvents()
        {

            ContentService.Published += ContentService_Published;
            ContentService.Saving += ContentService_OnSaving;
            ContentService.SendingToPublish += ContentService_SendingToPublish;
            ContentService.Trashing += ContentService_Trashing;
            ContentService.Moving += ContentService_Moving;
            ContentService.Publishing += ContentService_Publishing;
            //CustomInitialize();
        }

        private void ContentService_Publishing(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.ContentPublishingEventArgs publishingEventArgs)
        {
            try
            {
                //foreach (var e in publishingEventArgs.PublishedEntities)
                //{
                //    if (e.Id > 0 && e.Published && ConfigurationModel.WebsiteContentTypesList.Contains(e.ContentType.Alias))
                //    {
                //        var content = ServiceUtility.UmbracoHelper.GetById(e.Id);
                //        if (content.NodeExists())
                //        {

                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                //LogHelper.Error<string>("XrmPath.Web caught error on Global.asax.cs ContentService_Trashing()", ex);
            }
        }

        private void ContentService_Moving(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.MoveEventArgs<IContent> movingEventArgs)
        {
            try
            {

                //foreach (var e in movingEventArgs.MoveInfoCollection.Where(i => ConfigurationModel.WebsiteContentTypesList.Contains(i.Entity.ContentType.Alias)))
                //{
                //    //only loop through valid website content types
                //    if (e.Entity.Published && e.Entity.Id > 0)
                //    {
                //        var content = ServiceUtility.UmbracoHelper.GetById(e.Entity.Id);
                //        if (content.NodeExists())
                //        {

                //        }
                //    }

                //}
            }
            catch (Exception ex)
            {
                //LogHelper.Error<string>("XrmPath.Web caught error on Global.asax.cs ContentService_Moving()", ex);
            }
        }

        private void ContentService_Trashing(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.MoveEventArgs<IContent> trashEventArgs)
        {
            try
            {
                //foreach (var e in trashEventArgs.MoveInfoCollection)
                //{

                //}
            }
            catch (Exception ex)
            {
                //LogHelper.Error<string>("XrmPath.Web caught error on Global.asax.cs ContentService_Trashing()", ex);
            }
        }

        private void ContentService_SendingToPublish(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.SendToPublishEventArgs<IContent> sendToPublishEventArgs)
        {
            //var e = sendToPublishEventArgs.Entity;
            //if (e.Id > 0)
            //{
            //    //e.ReleaseDate = DateTime.Now.AddHours(24);
            //    //sender.Save(e);
            //}
        }

        private void ContentService_OnSaving(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.SaveEventArgs<IContent> saveEventArgs)
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
                        //var title = e.HasProperty(UmbracoCustomFields.Title) ? (e.GetValue(UmbracoCustomFields.Title)?.ToString() ?? "") : "";
                        //var description = e.HasProperty(UmbracoCustomFields.Description) ? (e.GetValue(UmbracoCustomFields.Description)?.ToString() ?? "") : "";
                        //var metaDescription = e.HasProperty(UmbracoCustomFields.MetaDescription) ? (e.GetValue(UmbracoCustomFields.MetaDescription)?.ToString() ?? "") : "";
                        //var metaKeywords = e.HasProperty(UmbracoCustomFields.MetaKeywords) ? (e.GetValue(UmbracoCustomFields.MetaKeywords)?.ToString() ?? "") : "";
                        //var dateNow = DateTime.Now.ToString("MMMM dd, yyyy");

                        //if (metaDescription.IndexOf(dateNow, StringComparison.Ordinal) == -1 && !string.IsNullOrEmpty(description))
                        //{
                        //    if (e.HasProperty(UmbracoCustomFields.MetaDescription))
                        //    {
                        //        e.SetValue(UmbracoCustomFields.MetaDescription, dateNow + " - " + description);
                        //    }
                        //}

                        //if (metaKeywords.IndexOf(title, StringComparison.Ordinal) == -1)
                        //{
                        //    if (e.HasProperty(UmbracoCustomFields.MetaKeywords))
                        //    {
                        //        e.SetValue(UmbracoCustomFields.MetaKeywords, metaKeywords + (!string.IsNullOrEmpty(metaKeywords) ? "," : "") + title);
                        //    }
                        //}

                    }
                    catch (Exception ex)
                    {
                        //LogHelper.Error<string>("XrmPath.Web caught error on Global.asax.cs ContentService_OnSaving()", ex);
                    }
                }
            }
        }

        private void ContentService_Published(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.PublishEventArgs<Umbraco.Core.Models.IContent> e)
        {

        }

        #endregion
    }
}
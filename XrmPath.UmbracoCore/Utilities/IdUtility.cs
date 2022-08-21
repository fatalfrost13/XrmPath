
using System;
using System.Collections.Generic;
using System.Linq;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Models;
using Umbraco.Web;
using System.Web;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace XrmPath.UmbracoCore.Utilities
{
    public static class IdUtility
    {

        public static IPublishedContent GetById(this UmbracoHelper umbracoHelper, int id)
        {
            //var content = umbHelper != null ? umbHelper.GetById(id) : Umbraco.Web.Composing.Current.UmbracoContext.Content.GetById(id);
            IPublishedContent content = null;
            if (Umbraco.Web.Composing.Current.UmbracoContext.Content != null)
            {
                content = Umbraco.Web.Composing.Current.UmbracoContext.Content.GetById(id);
            }
            else 
            {
                content = umbracoHelper.GetById(id);
            }
            return content;
        }

        public static int GetNodeIdFromUrl(string url)
        {
            var nodeId = 0;

            //var content = ServiceUtility.UmbracoHelper..ContentCache.GetByRoute(url);
            var content = Umbraco.Web.Composing.Current.UmbracoContext.Content.GetByRoute(url);
            if (content.NodeExists())
            {
                nodeId = content.Id;
            }
            return nodeId;
        }

    }
}
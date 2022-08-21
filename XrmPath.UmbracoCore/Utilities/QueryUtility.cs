using System.Collections.Generic;
using System.Linq;
using XrmPath.UmbracoCore.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace XrmPath.UmbracoCore.Utilities
{

    public static class QueryUtility
    {
        public static IPublishedContent GetNodeByUrl(string url)
        {
            var node = Umbraco.Web.Composing.Current.UmbracoContext.Content.GetByRoute(url);
            return node;
        }

        /// <summary>
        /// </summary>
        /// <param name="alias">Single alias</param>
        /// <returns></returns>
        public static IEnumerable<IPublishedContent> GetPublishedContentByTypeSingle(string alias = "")
        {
            var nodeList = !string.IsNullOrEmpty(alias) ? ServiceUtility.UmbracoHelper.ContentAtXPath($"//{alias}") : Enumerable.Empty<IPublishedContent>();
            return nodeList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aliases">Aliases comma separated</param>
        /// <returns></returns>
        public static IEnumerable<IPublishedContent> GetPublishedContentByType(string aliases = "")
        {
            if (aliases.Contains(","))
            {
                var aliasList = aliases.Split(',');
                var nodeList = aliasList.SelectMany(GetPublishedContentByTypeSingle).ToList();
                return nodeList;
            }

            if (string.IsNullOrEmpty(aliases))
            {
                var nodeList = new List<IPublishedContent>();
                var rootNodes = ServiceUtility.UmbracoHelper.ContentAtRoot();
                foreach (var rootNode in rootNodes)
                {
                    var rootSubNodes = rootNode.DescendantsOrSelf().ToList();
                    if (rootSubNodes.Any())
                    {
                        nodeList.AddRange(rootSubNodes);
                    }
                }

                return nodeList;
                //return Enumerable.Empty<IPublishedContent>();
            }

            return GetPublishedContentByTypeSingle(aliases);
        }

        public static IPublishedContent GetPageByUniqueId(string uniqueId = "", string aliases = "")
        {
            if (string.IsNullOrEmpty(aliases))
            {
                aliases = ConfigurationModel.WebsiteContentTypes;
            }
            if (!string.IsNullOrEmpty(uniqueId))
            {
                var uniquePage = GetPublishedContentByType(aliases).FirstOrDefault(i => i.GetContentValue(UmbracoCustomFields.UniqueId) == uniqueId);
                return uniquePage;

            }
            else
            {
                //return homepage
                var homePage = GetPublishedContentByType(UmbracoCustomTypes.HomePage).First();
                return homePage;
            }
        }

        public static IPublishedContent GetNodeByFieldValue(string docTypeAliases, string fieldAlias, string fieldValue)
        {
            var nodeByFieldValue = GetPublishedContentByType(docTypeAliases).FirstOrDefault(i => !string.IsNullOrEmpty(fieldValue) && i.GetContentValue(fieldAlias) == fieldValue);
            return nodeByFieldValue;
        }

    }
}
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using XrmPath.UmbracoCore.Definitions;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.Utilities
{
    public class QueryUtility: BaseInitializer
    {
        /// <summary>
        /// Dependencies: Logger(optional), UmbracoHelper
        /// </summary>
        /// <param name="serviceUtil"></param>
        public QueryUtility(ServiceUtility serviceUtil) : base(serviceUtil) { }

        /// <summary>
        /// </summary>
        /// <param name="alias">Single alias</param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> GetPublishedContentByTypeSingle(string alias = "")
        {
            if (umbracoHelper == null) {
                return Enumerable.Empty<IPublishedContent>();
            }
            var nodeList = !string.IsNullOrEmpty(alias) ? umbracoHelper.ContentAtXPath($"//{alias}") : Enumerable.Empty<IPublishedContent>();
            return nodeList ?? Enumerable.Empty<IPublishedContent>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aliases">Aliases comma separated</param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> GetPublishedContentByType(string aliases = "")
        {
            if (aliases.Contains(","))
            {
                var aliasList = aliases.Split(',');
                var nodeList = aliasList.SelectMany(GetPublishedContentByTypeSingle).ToList();
                return nodeList;
            }

            if (string.IsNullOrEmpty(aliases) && umbracoHelper != null)
            {
                var nodeList = new List<IPublishedContent>();
                var rootNodes = umbracoHelper.ContentAtRoot();
                if (rootNodes != null) {
                    foreach (var rootNode in rootNodes)
                    {
                        var rootSubNodes = rootNode.DescendantsOrSelf().ToList();
                        if (rootSubNodes.Any())
                        {
                            nodeList.AddRange(rootSubNodes);
                        }
                    }
                }
                return nodeList;
            }

            return GetPublishedContentByTypeSingle(aliases);
        }

        public IPublishedContent? GetPageByUniqueId(string uniqueId = "", string aliases = "")
        {
            if (string.IsNullOrEmpty(aliases))
            {
                aliases = ConfigurationModel.WebsiteContentTypes;
            }
            if (!string.IsNullOrEmpty(uniqueId) && pcUtil != null)
            {
                var uniquePage = GetPublishedContentByType(aliases).FirstOrDefault(i => pcUtil.GetContentValue(i, UmbracoCustomFields.UniqueId) == uniqueId);
                return uniquePage;

            }
            else
            {
                //return homepage
                var homePage = GetPublishedContentByType(UmbracoCustomTypes.HomePage).First();
                return homePage;
            }
        }

        public IPublishedContent? GetNodeByFieldValue(string docTypeAliases, string fieldAlias, string fieldValue)
        {
            if (pcUtil == null) 
            {
                return null;
            }
            var nodeByFieldValue = GetPublishedContentByType(docTypeAliases).FirstOrDefault(i => !string.IsNullOrEmpty(fieldValue) && pcUtil.GetContentValue(i, fieldAlias) == fieldValue);
            return nodeByFieldValue;
        }
    }
}

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using XrmPath.UmbracoUtils;
using XrmPath.UmbracoUtils.Models;

namespace XrmPath.Web.UmbracoUtils
{
    public class QueryUtility
    {
        protected readonly UmbracoHelper? _umbracoHelper;
        protected readonly PublishedContentUtility? _pcUtil;
        public QueryUtility(PublishedContentUtility pcUtil)
        {
            if (pcUtil != null && _pcUtil == null)
            {
                _pcUtil = pcUtil;
                if (_umbracoHelper == null)
                {
                    _umbracoHelper = _pcUtil.GetUmbracoHelper();
                }
            }
            
        }

        /// <summary>
        /// </summary>
        /// <param name="alias">Single alias</param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> GetPublishedContentByTypeSingle(string alias = "")
        {
            var nodeList = !string.IsNullOrEmpty(alias) ? _umbracoHelper?.ContentAtXPath($"//{alias}") : Enumerable.Empty<IPublishedContent>();
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

            if (string.IsNullOrEmpty(aliases))
            {
                var nodeList = new List<IPublishedContent>();
                var rootNodes = _umbracoHelper?.ContentAtRoot();
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
                //return Enumerable.Empty<IPublishedContent>();
            }

            return GetPublishedContentByTypeSingle(aliases);
        }

        public IPublishedContent? GetPageByUniqueId(string uniqueId = "", string aliases = "")
        {
            if (string.IsNullOrEmpty(aliases))
            {
                aliases = ConfigurationModel.WebsiteContentTypes;
            }
            if (!string.IsNullOrEmpty(uniqueId))
            {
                var uniquePage = GetPublishedContentByType(aliases).FirstOrDefault(i => _pcUtil?.GetContentValue(i, UmbracoCustomFields.UniqueId) == uniqueId);
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
            var nodeByFieldValue = GetPublishedContentByType(docTypeAliases).FirstOrDefault(i => !string.IsNullOrEmpty(fieldValue) && _pcUtil?.GetContentValue(i, fieldAlias) == fieldValue);
            return nodeByFieldValue;
        }
    }
}

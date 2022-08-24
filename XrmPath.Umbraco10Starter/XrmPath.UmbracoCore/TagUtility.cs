using Microsoft.IdentityModel.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Definitions;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.Utilities
{
    /// <summary>
    /// Dependencies: Logger(optional), UmbracoHelper
    /// </summary>
    /// <param name="serviceUtil"></param>
    public class TagUtility : BaseInitializer
    {
        public TagUtility(ServiceUtility serviceUtil) : base(serviceUtil) { }

        public IEnumerable<IPublishedContent> GetAllTaggedNodes(int id, string alias, string docTypes = "")
        {
            var taggedNodes = Enumerable.Empty<IPublishedContent>();
            try
            {
                if (string.IsNullOrEmpty(docTypes))
                {
                    //var taggedNodes = new List<IPublishedContent>();
                    var rootNodes = umbracoHelper?.ContentAtRoot().ToList() ?? new List<IPublishedContent>();
                    if (rootNodes.Any())
                    {
                        taggedNodes = rootNodes.SelectMany(child => pcUtil?.FindAllNodes(child, ConfigurationModel.WebsiteContentTypesSet).Where(i => (bool)pcUtil?.GetNodeList(i, alias)?.Select(x => x.Id)?.Contains(id)));
                    }
                }
                else
                {
                    taggedNodes = queryUtil?.GetPublishedContentByType(docTypes).Where(i => (bool)pcUtil?.GetNodeList(i, alias).Select(x => x.Id).Contains(id));
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Warning($"XrmPath.UmbracoCore caught error on TagHelper.GetAllTaggedNodes(): {ex}. URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Warning($"XrmPath.UmbracoCore caught error on TagHelper.GetAllTaggedNodes(): {ex}. URL Info: {UrlUtility.GetCurrentUrl()}");
            }
            return taggedNodes ?? Enumerable.Empty<IPublishedContent>();
        }

        public bool IsTaggedWithKeyword(IPublishedContent content, string searchTerm = "", string alias = "")
        {
            var tagged = false;
            if (string.IsNullOrEmpty(alias))
            {
                alias = UmbracoCustomFields.KeywordTags;
            }
            var keywordTags = pcUtil?.GetContentValue(content, alias) ?? "";
            var tagList = keywordTags.StringToSet().Where(i => !string.IsNullOrEmpty(i)).Select(i => i.ToLower().Trim()).ToList();
            if (tagList.Any() && tagList.Contains(searchTerm.ToLower()))
            {
                tagged = true;
            }

            return tagged;
        }
    }
}

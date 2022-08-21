using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Models;
using XrmPath.UmbracoCore.Utilities;

namespace XrmPath.UmbracoCore.Helpers
{
    public static class TagHelper
    {
        public static IEnumerable<IPublishedContent> GetAllTaggedNodes(int id, string alias, string docTypes = "")
        {
            var taggedNodes = Enumerable.Empty<IPublishedContent>();
            try
            {
                if (string.IsNullOrEmpty(docTypes))
                {
                    //var taggedNodes = new List<IPublishedContent>();
                    var rootNodes = ServiceUtility.UmbracoHelper.ContentAtRoot().ToList();
                    if (rootNodes.Any())
                    {
                        taggedNodes = rootNodes.SelectMany(child => child.FindAllNodes(ConfigurationModel.WebsiteContentTypesSet).Where(i => i.GetNodeList(alias).Select(x => x.Id).Contains(id)));
                    }
                }
                else
                {
                    taggedNodes = QueryUtility.GetPublishedContentByType(docTypes).Where(i => i.GetNodeList(alias).Select(x => x.Id).Contains(id));
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Warning($"XrmPath.UmbracoCore caught error on TagHelper.GetAllTaggedNodes(): {ex}. URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Warning($"XrmPath.UmbracoCore caught error on TagHelper.GetAllTaggedNodes(): {ex}. URL Info: {UrlUtility.GetCurrentUrl()}");
            }
            return taggedNodes;
        }

        public static bool IsTaggedWithKeyword(this IPublishedContent content, string searchTerm = "", string alias = "")
        {
            var tagged = false;
            if (string.IsNullOrEmpty(alias))
            {
                alias = UmbracoCustomFields.KeywordTags;
            }
            var keywordTags = content.GetContentValue(alias);
            var tagList = keywordTags.StringToSet().Where(i => !string.IsNullOrEmpty(i)).Select(i => i.ToLower().Trim()).ToList();
            if (tagList.Any() && tagList.Contains(searchTerm.ToLower()))
            {
                tagged = true;
            }

            return tagged;
        }
    }
}

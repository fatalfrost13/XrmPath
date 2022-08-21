
using System;
using System.Collections.Generic;
using System.Linq;
using XrmPath.Helpers.Utilities;
using XrmPath.Web.Models;
using Examine;
using RJP.MultiUrlPicker.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using System.Web;

namespace XrmPath.Web.Helpers.UmbracoHelpers
{
    public static class CustomUmbracoHelper
    {
        public static UmbracoHelper GetUmbracoHelper()
        {
            ContextHelper.EnsureUmbracoContext();
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            return umbracoHelper;
        }

        public static IPublishedContent GetById(this UmbracoHelper umbracoHelper, int id)
        {
            var content = UmbracoContext.Current == null ? umbracoHelper.UmbracoContext.ContentCache.GetById(id) : UmbracoContext.Current.ContentCache.GetById(id);
            return content;
        }

        public static IPublishedContent GetCurrentPage()
        {
            ContextHelper.EnsureUmbracoContext();
            var currentPage = UmbracoContext.Current.PublishedContentRequest.PublishedContent;
            return currentPage;
        }


        public static int GetNodeIdFromUrl(string url)
        {
            var nodeId = 0;
            var content = ServiceUtility.UmbracoHelper.UmbracoContext.ContentCache.GetByRoute(url);
            if (content.NodeExists())
            {
                nodeId = content.Id;
            }
            return nodeId;
        }

        public static List<UrlPicker> GetBreadcrumb(IPublishedContent content)
        {
            var nodeList = content.Path.Split(',').Select(i => ServiceUtility.UmbracoHelper.GetById(int.Parse(i)))
                .Where(i => i != null && i.Level > 1)
                .Where(i => ConfigurationModel.WebsiteContentTypesList.Contains(i.DocumentTypeAlias) || !string.IsNullOrEmpty(i.GetContentValue(UmbracoCustomFields.UrlPicker)));
            var breadCrumbList = nodeList.Select(i => new UrlPicker
            {
                Title = i.GetTitle(),
                NewWindow = i.GetTarget() == "_blank",
                Url = i.GetUrl(),
                LinkType = LinkType.Content,
                NodeId = i.Id
            }).ToList();
            return breadCrumbList;
        }

        public static void RebuildUmbracoCache()
        {
            ServiceUtility.ContentService.BuildXmlCache();
        }

        public static void RebuildUmbracoIndexes()
        {
            ExamineManager.Instance.IndexProviderCollection.ToList().ForEach(index => index.RebuildIndex());
        }

        public static IEnumerable<IPublishedContent> GetAllTaggedNodes(int id, string alias, string docTypes = "")
        {
            var taggedNodes = Enumerable.Empty<IPublishedContent>();
            try
            {
                if (string.IsNullOrEmpty(docTypes)) { 
                    //var taggedNodes = new List<IPublishedContent>();
                    var rootNodes = ServiceUtility.UmbracoHelper.TypedContentAtRoot().ToList();
                    if (rootNodes.Any())
                    {
                        taggedNodes = rootNodes.SelectMany(child => child.FindAllNodes(ConfigurationModel.WebsiteContentTypesSet).Where(i => i.GetNodeList(alias).Select(x => x.Id).Contains(id)));
                    }
                }else
                {
                    taggedNodes = QueryUtility.GetPublishedContentByType(docTypes).Where(i => i.GetNodeList(alias).Select(x => x.Id).Contains(id));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn<IPublishedContent>($"XrmPath.Web caught error on CustomUmbracoHelper.GetAllTaggedNodes(): {ex}. URL Info: {UrlUtility.GetCurrentUrl()}");
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


        public static string GetFirstNodeDomain(string uniqueId, string alias)
        {
            //var exportHome = QueryUtility.GetPageByUniqueId(UmbracoCustomLookups.ExportHome, UmbracoCustomTypes.ExportHome);
            var domain = string.Empty;
            var node = QueryUtility.GetPageByUniqueId(uniqueId, alias);
            var domainList = umbraco.library.GetCurrentDomains(node.Id);
            var currentUrl = HttpContext.Current?.Request.Url.ToString();
            if (domainList != null && domainList.Any())
            {
                var firstDomain = domainList.FirstOrDefault()?.Name ?? string.Empty;
                if (!string.IsNullOrEmpty(firstDomain) && !currentUrl.StartsWith(firstDomain))
                {
                    //if currenturl is the same as first domain, pass empty string and use relative path
                    domain = firstDomain ?? string.Empty;
                    if (!domain.StartsWith("https://") && !domain.StartsWith("http://"))
                    { 
                        if (currentUrl.StartsWith("https://"))
                        {
                            domain = $"https://{domain}";
                        }
                        else
                        {
                            domain = $"http://{domain}";
                        }
                    }
                }
                return domain;
            }
            return domain;
        }

        public static string GetHomeUniqueId(IPublishedContent content = null)
        {
            if (content == null)
            {
                content = GetCurrentPage();
            }
            var uniqueId = string.Empty;
            var homeNode = content.AncestorOrSelf(1);
            if (homeNode != null)
            {
                uniqueId = homeNode.GetContentValue(UmbracoCustomFields.UniqueId);
            }

            var forceHomeId = HttpContext.Current?.Request.QueryString["homeId"] ?? string.Empty;
            if (!string.IsNullOrEmpty(forceHomeId))
            {
                uniqueId = forceHomeId;
            }

            return uniqueId;
        }
    }
}
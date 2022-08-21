using System;
using System.Linq;
using System.Web;
using XrmPath.Web.Helpers.UmbracoHelpers;
using XrmPath.Web.Models;
using Umbraco.Core.Models;
using XrmPath.Helpers.Utilities;
using XrmPath.Web.Helpers.Utils;
using XrmPath.Web.Models.CacheModels;
using Umbraco.Core.Logging;
using System.Collections.Generic;
using Umbraco.Web;

namespace XrmPath.Web.Helpers
{

    public static class PageHelper
    {

        private static readonly DateTime viewsEndDate = DateTime.UtcNow.AddHours(1); //add time to compensate for time difference so on refresh the new views are applied no matter what.
        private static readonly DateTime viewsStartDate = DateTime.UtcNow.AddMonths(-2);

        public static PageModel GetPageModel(IPublishedContent content)
        {
            try
            {
                var pageModel = new PageModel
                {
                    Title = content.GetTitle(),
                    Description = content.GetContentValue(UmbracoCustomFields.Description),
                };
                return pageModel;
            }
            catch(Exception ex)
            {
                LogHelper.Error<PageModel>($"XrmPath.Web caught error on PageHelper.GetPageModel(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
                return new PageModel();
            }
        }

        public static MetaData GetMetaData(IPublishedContent content)
        {
            try
            {
                var descriptionFields = $"{UmbracoCustomFields.MetaDescription},{UmbracoCustomFields.Description}";
                var title = content.GetTitle();
                var description = content.GetContentValue(descriptionFields.StringToSet());
                //var keywordTags = content.GetContentValue(UmbracoCustomFields.KeywordTags);

                var keywords = GetKeywords(content);

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(keywords))
                {
                    var homePage = QueryUtility.GetPublishedContentByType(UmbracoCustomTypes.HomePage).First();
                    if (string.IsNullOrEmpty(title))
                    {
                        title = homePage.GetTitle();
                    }
                    if (string.IsNullOrEmpty(description))
                    {
                        description = homePage.GetContentValue(descriptionFields.StringToSet());
                    }
                    if (string.IsNullOrEmpty(keywords))
                    {
                        //see if there are any
                        keywords = GetKeywords(homePage);
                    }

                }

                var metaData = new MetaData
                {
                    Title = title,
                    Description = description,
                    Keywords = keywords
                };
                return metaData;
            }
            catch (Exception ex)
            {
                LogHelper.Error<MetaData>($"XrmPath.Web caught error on PageHelper.GetMetaData(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
                return new MetaData();
            }
        }

        private static string GetKeywords(IPublishedContent content)
        {
            var keywords = string.Empty;
            try
            {
                //creates keywords based on KeywordTags and MetaKeywords fields
                keywords = content.GetContentValue(UmbracoCustomFields.KeywordTags);
                var metaKeywords = content.GetContentValue(UmbracoCustomFields.MetaKeywords);
                if (!string.IsNullOrEmpty(metaKeywords))
                {
                    keywords = !string.IsNullOrEmpty(keywords) ? $"{keywords},{metaKeywords}" : metaKeywords;
                }

                //ensure they are distinct
                var keywordList = keywords.StringToSet().Distinct();
                keywords = string.Join(",", keywordList);
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on PageHelper.GetKeywords(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return keywords;
        }

        public static bool UntrackableDestination(int id)
        {
            var untrackable = false;

            try
            {
                if (id > 0)
                {
                    var content = ServiceUtility.UmbracoHelper.GetById(id);
                    untrackable = content.Url.IndexOf("http://", StringComparison.Ordinal) >= 0 || content.Url.IndexOf("https://", StringComparison.Ordinal) >= 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on PageHelper.UntrackableDestination(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return untrackable;
        }

        public static string GetRedirectUrl(string url)
        {
            var redirectUrl = string.Empty;

            try
            {
                var requestDomain = WebUtility.GetDomain(url);
                var oldRelativeUrl = WebUtility.GetRelativeUrl(url);
                var redirectUrlList = QueryUtility.GetPublishedContentByType(UmbracoCustomTypes.RedirectItem);

                var autoGeneratedFolder = QueryUtility.GetPageByUniqueId(UmbracoCustomLookups.AutoGeneratedRedirects, UmbracoCustomTypes.RedirectFolder);
                var matchingNodesAuto = new List<IPublishedContent>();
                if (autoGeneratedFolder != null)
                {
                    matchingNodesAuto = autoGeneratedFolder.FindAllNodes(UmbracoCustomTypes.RedirectItem)
                        .Where(i => WebUtility.GetRelativeUrl(url).Equals(WebUtility.GetRelativeUrl(i.GetContentValue(UmbracoCustomFields.OldUrl))))
                        .ToList();
                }

                var redirectNodeList = redirectUrlList
                    .Select(i => new { node = i, oldurl = i.GetContentValue(UmbracoCustomFields.OldUrl) })
                    .Where(i => !string.IsNullOrEmpty(i.oldurl) && i.oldurl != "#")
                    .Where(i => WebUtility.GetRelativeUrl(i.oldurl.ToLower()).Equals(oldRelativeUrl))
                    .Select(i => i.node)
                    .ToList();

                var findNodes = redirectNodeList.Where(i => !matchingNodesAuto.Select(x => x.Id).Contains(i.Id)).ToList();  //find permanent nodes first
                if (!findNodes.Any())
                {
                    findNodes = redirectNodeList;
                }

                var redirectNode = findNodes.FirstOrDefault();

                if (redirectNode != null)
                {
                    var sourceNodeUrl = redirectNode.GetContentPicker(UmbracoCustomFields.SourceNode)?.GetUrl() ?? string.Empty;
                    //var newUrl = redirectNode.GetUrl(UmbracoCustomFields.SourceNode);
                    if (sourceNodeUrl.StartsWith("/"))
                    {
                        redirectUrl = sourceNodeUrl;
                    }
                    else if(sourceNodeUrl.StartsWith("http"))
                    {
                        var newDomain = WebUtility.GetDomain(sourceNodeUrl);
                        if (!string.IsNullOrEmpty(requestDomain) && !string.IsNullOrEmpty(newDomain) && requestDomain.ToLower().Equals(newDomain.ToLower()))
                        {
                            redirectUrl = WebUtility.GetRelativeUrl(redirectNode.GetUrl(UmbracoCustomFields.SourceNode));
                        }
                        else
                        {
                            redirectUrl = sourceNodeUrl;
                        }
                    }
                    else
                    {
                        redirectUrl = WebUtility.GetRelativeUrl(redirectNode.GetUrl(UmbracoCustomFields.SourceNode));
                    }
                
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on PageHelper.GetRedirectUrl({url}). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return redirectUrl;
        }

        public static string GetUrlFromParentLookup(string url)
        {
            var relativeUrl = WebUtility.GetRelativeUrl(url);
            var directoryList = relativeUrl.Split('/').Where(i => !string.IsNullOrEmpty(i)).ToList();
            var directoryCount = directoryList.Count;
            for(var i = 0; i <= directoryCount - 1; i++) {
                var urlPath = string.Empty;
                var currentIndex = (directoryCount - 1 ) - i;
                for(var x = 0; x <= currentIndex; x++)
                {
                    urlPath += "/" + directoryList[x];
                }

                //once the URL path has been built. do a lookup
                var parentUrl = GetRedirectUrl(urlPath);
                if (!string.IsNullOrEmpty(parentUrl)) {

                    //parent redirect has been found. now create the redirect URL
                    var indexMarker = url.LastIndexOf(urlPath) + urlPath.Length;
                    if (indexMarker >= 0) { 
                        var appendUrl = url.Substring(indexMarker, url.Length - indexMarker);
                        if (parentUrl.EndsWith("/") && appendUrl.StartsWith("/")) {
                            appendUrl = appendUrl.Substring(1, appendUrl.Length - 1);
                        }
                        var redirectUrl = $"{parentUrl}{appendUrl}";
                        var validateUrl = WebUtility.GetRelativeUrl(redirectUrl);
                        var validateNode = QueryUtility.GetNodeByUrl(validateUrl);
                        if (validateNode.NodeExists()) {
                            return redirectUrl;
                        }
                    }
                }
            }

            return null;
        }

        public static bool HandleUrlChange(IPublishedContent content, string note = "")
        {
            var createdRedirect = false;

            //first check if record exists
            var autoGeneratedFolder = QueryUtility.GetPageByUniqueId(UmbracoCustomLookups.AutoGeneratedRedirects, UmbracoCustomTypes.RedirectFolder);
            if (autoGeneratedFolder != null)
            {
                var matchingNodes = autoGeneratedFolder.FindAllNodes(UmbracoCustomTypes.RedirectItem).Select(i => new { ContentNode = i, SourceNode = i.GetContentPicker(UmbracoCustomFields.SourceNode) })
                    //.Where(i => i.sourceNode != null && i.sourceNode.Id == content.Id)
                    .Where(i => WebUtility.GetRelativeUrl(content.Url).Equals(WebUtility.GetRelativeUrl(i.ContentNode.GetContentValue(UmbracoCustomFields.OldUrl))))
                    .ToList();

                if (!matchingNodes.Any())
                {
                    //add new url redirect
                    //var redirectName = $"{content.Name} - {DateTime.Now.ToString("yyyy-MM-dd h:mm tt")}";
                    var oldUrl = WebUtility.GetRelativeUrl(content.Url);
                    var sourcePickerId = ServiceUtility.UmbracoHelper.GetById(content.Id).GetUdiString();
                    var redirectName = oldUrl;
                    var redirectContent = ServiceUtility.ContentService.CreateContent(redirectName, autoGeneratedFolder.Id, UmbracoCustomTypes.RedirectItem);
                    redirectContent.SetValue(UmbracoCustomFields.OldUrl, oldUrl);
                    redirectContent.SetValue(UmbracoCustomFields.SourceNode, sourcePickerId);
                    redirectContent.SetValue(UmbracoCustomFields.History, note);
                    ServiceUtility.ContentService.SaveAndPublishWithStatus(redirectContent);
                    createdRedirect = true;
                }
                else
                {
                    //update existing record
                    var matchingNode = matchingNodes.OrderByDescending(i => i.ContentNode.CreateDate).FirstOrDefault(i => i.ContentNode.NodeExists());
                    var removeNodes = matchingNodes.Where(i => i.ContentNode.Id != matchingNode.ContentNode.Id && i.ContentNode.NodeExists());
                    
                    if (matchingNode != null)
                    {
                        var updateContent = ServiceUtility.ContentService.GetById(matchingNode.ContentNode.Id);
                        var existingNote = updateContent.GetContentValue(UmbracoCustomFields.History);
                        var updateNote = string.Empty;
                        if (!string.IsNullOrEmpty(existingNote))
                        {
                            updateNote = $"\n{existingNote}";
                        }
                        updateNote = $"{note}{updateNote}";

                        var oldUrl = WebUtility.GetRelativeUrl(content.Url);
                        var sourcePickerId = ServiceUtility.UmbracoHelper.GetById(content.Id).GetUdiString();
                        //var redirectName = $"{content.Name} - {DateTime.Now.ToString("yyyy-MM-dd h:mm tt")}";
                        var redirectName = oldUrl;
                        updateContent.Name = redirectName;
                        updateContent.SetValue(UmbracoCustomFields.OldUrl, oldUrl);
                        updateContent.SetValue(UmbracoCustomFields.SourceNode, sourcePickerId);
                        updateContent.SetValue(UmbracoCustomFields.History, updateNote);
                        ServiceUtility.ContentService.SaveAndPublishWithStatus(updateContent);
                        
                        foreach(var removeNode in removeNodes)
                        {
                            //delete other duplicates
                            var removeContent = ServiceUtility.ContentService.GetById(removeNode.ContentNode.Id);
                            ServiceUtility.ContentService.Delete(removeContent);
                        }

                        createdRedirect = true;
                    }
                }
            }

            return createdRedirect;
        }

    }


}
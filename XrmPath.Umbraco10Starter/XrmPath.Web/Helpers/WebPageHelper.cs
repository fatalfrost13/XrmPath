using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Logging;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.BaseServices;
using XrmPath.UmbracoCore.Definitions;
using XrmPath.UmbracoCore.Models.Definitions;
using XrmPath.UmbracoCore.Utilities;
using XrmPath.Web.Models;

namespace XrmPath.Web.Helpers
{
    /// <summary>
    /// Dependencies: Logger(optional), UmbracoHelper
    /// </summary>
    /// <param name="serviceUtil"></param>
    public class WebPageHelper : BaseInitializer
    {
        public WebPageHelper(ServiceUtility serviceUtil) : base(serviceUtil) { }

        public WebPageModel GetPageModel(IPublishedContent content)
        {
            try
            {
                var pageModel = new WebPageModel
                {
                    Title = pcUtil?.GetTitle(content) ?? (content.Name ?? ""),
                    Description = pcUtil?.GetContentValue(content, UmbracoCustomFields.Description) ?? "",
                };
                return pageModel;
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on PageHelper.GetPageModel(). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.Web caught error on PageHelper.GetPageModel(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
                return new WebPageModel();
            }
        }

        public MetaDataModel GetMetaData(IPublishedContent content)
        {
            try
            {
                var descriptionFields = $"{UmbracoCustomFields.MetaDescription},{UmbracoCustomFields.Description}";
                var title = pcUtil?.GetTitle(content) ?? (content.Name ?? "");
                var description = pcUtil?.GetContentValue(content, descriptionFields.StringToSet()) ?? "";
                //var keywordTags = content.GetContentValue(UmbracoCustomFields.KeywordTags);

                var keywords = GetKeywords(content);

                if ((string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(keywords)) && queryUtil != null)
                {
                    var homePage = queryUtil.GetPublishedContentByType(UmbracoCustomTypes.HomePage).First();
                    if (string.IsNullOrEmpty(title))
                    {
                        title = pcUtil?.GetTitle(homePage) ?? (homePage?.Name ?? "");
                    }
                    if (string.IsNullOrEmpty(description))
                    {
                        description = pcUtil?.GetContentValue(homePage, descriptionFields.StringToSet()) ?? "";
                    }
                    if (string.IsNullOrEmpty(keywords))
                    {
                        //see if there are any
                        keywords = GetKeywords(homePage) ?? "";
                    }

                }

                var metaData = new MetaDataModel
                {
                    Title = title,
                    Description = description,
                    Keywords = keywords
                };
                return metaData;
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on PageHelper.GetMetaData(). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.Web caught error on PageHelper.GetMetaData(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
                return new MetaDataModel();
            }
        }

        private string GetKeywords(IPublishedContent? content)
        {
            var keywords = string.Empty;

            if (pcUtil == null || content == null) {
                return keywords;
            }

            try
            {
                //creates keywords based on KeywordTags and MetaKeywords fields
                keywords = pcUtil.GetContentValue(content, UmbracoCustomFields.KeywordTags);
                var metaKeywords = pcUtil.GetContentValue(content, UmbracoCustomFields.MetaKeywords);
                if (!string.IsNullOrEmpty(metaKeywords))
                {
                    keywords = !string.IsNullOrEmpty(keywords) ? $"{keywords},{metaKeywords}" : metaKeywords;
                }

                //ensure they are distinct
                var keywordList = keywords?.StringToSet().Distinct() ?? Enumerable.Empty<string>();
                keywords = string.Join(",", keywordList);
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on PageHelper.GetKeywords(). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.Web caught error on PageHelper.GetKeywords(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return keywords ?? "";
        }

        public bool UntrackableDestination(int id)
        {
            var untrackable = false;

            if (umbracoHelper == null) {
                return untrackable;
            }

            try
            {
                if (id > 0)
                {
                    var content = umbracoHelper.Content(id);
                    untrackable = content?.Url().IndexOf("http://", StringComparison.Ordinal) >= 0 || content?.Url().IndexOf("https://", StringComparison.Ordinal) >= 0;
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on PageHelper.UntrackableDestination(). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.Web caught error on PageHelper.UntrackableDestination(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return untrackable;
        }

        public string GetRedirectUrl(string url)
        {
            var redirectUrl = string.Empty;

            if (pcUtil == null || queryUtil == null) {
                return redirectUrl;
            }

            try
            {
                var requestDomain = WebUtility.GetDomain(url);
                var oldRelativeUrl = WebUtility.GetRelativeUrl(url);
                var redirectUrlList = queryUtil.GetPublishedContentByType(UmbracoCustomTypes.RedirectItem);

                var autoGeneratedFolder = queryUtil.GetPageByUniqueId(UmbracoCustomLookups.AutoGeneratedRedirects, UmbracoCustomTypes.RedirectFolder);
                var matchingNodesAuto = new List<IPublishedContent>();
                if (autoGeneratedFolder != null)
                {
                    matchingNodesAuto = pcUtil.FindAllNodes(autoGeneratedFolder, UmbracoCustomTypes.RedirectItem)
                        .Where(i => WebUtility.GetRelativeUrl(url).Equals(WebUtility.GetRelativeUrl(pcUtil.GetContentValue(i, UmbracoCustomFields.OldUrl))))
                        .ToList();
                }

                var redirectNodeList = redirectUrlList?
                    .Select(i => new { node = i, oldurl = pcUtil.GetContentValue(i, UmbracoCustomFields.OldUrl) })
                    .Where(i => !string.IsNullOrEmpty(i.oldurl) && i.oldurl != "#")
                    .Where(i => WebUtility.GetRelativeUrl(i.oldurl.ToLower()).Equals(oldRelativeUrl))
                    .Select(i => i.node)
                    .ToList() ?? new List<IPublishedContent>();

                var findNodes = redirectNodeList.Where(i => matchingNodesAuto != null && !matchingNodesAuto.Select(x => x?.Id).Contains(i?.Id)).ToList();  //find permanent nodes first
                if (!findNodes.Any())
                {
                    findNodes = redirectNodeList;
                }

                var redirectNode = findNodes.FirstOrDefault();

                if (redirectNode != null)
                {
                    var sourceNodeUrl = pcUtil.GetContentPicker(redirectNode,UmbracoCustomFields.SourceNode)?.Url() ?? string.Empty;
                    //var newUrl = redirectNode.GetUrl(UmbracoCustomFields.SourceNode);
                    if (sourceNodeUrl.StartsWith("/"))
                    {
                        redirectUrl = sourceNodeUrl;
                    }
                    else if (sourceNodeUrl.StartsWith("http"))
                    {
                        var newDomain = WebUtility.GetDomain(sourceNodeUrl);
                        if (!string.IsNullOrEmpty(requestDomain) && !string.IsNullOrEmpty(newDomain) && requestDomain.ToLower().Equals(newDomain.ToLower()))
                        {
                            redirectUrl = WebUtility.GetRelativeUrl(pcUtil.GetUrl(redirectNode, UmbracoCustomFields.SourceNode));
                        }
                        else
                        {
                            redirectUrl = sourceNodeUrl;
                        }
                    }
                    else
                    {
                        redirectUrl = WebUtility.GetRelativeUrl(pcUtil.GetUrl(redirectNode, UmbracoCustomFields.SourceNode));
                    }

                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on PageHelper.GetRedirectUrl({url}). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.Web caught error on PageHelper.GetRedirectUrl({url}). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return redirectUrl;
        }

        public string GetUrlFromParentLookup(string url)
        {
            var relativeUrl = WebUtility.GetRelativeUrl(url);
            var directoryList = relativeUrl.Split('/').Where(i => !string.IsNullOrEmpty(i)).ToList();
            var directoryCount = directoryList.Count;
            for (var i = 0; i <= directoryCount - 1; i++)
            {
                var urlPath = string.Empty;
                var currentIndex = (directoryCount - 1) - i;
                for (var x = 0; x <= currentIndex; x++)
                {
                    urlPath += "/" + directoryList[x];
                }

                //once the URL path has been built. do a lookup
                var parentUrl = GetRedirectUrl(urlPath);
                if (!string.IsNullOrEmpty(parentUrl))
                {

                    //parent redirect has been found. now create the redirect URL
                    var indexMarker = url.LastIndexOf(urlPath) + urlPath.Length;
                    if (indexMarker >= 0)
                    {
                        var appendUrl = url.Substring(indexMarker, url.Length - indexMarker);
                        if (parentUrl.EndsWith("/") && appendUrl.StartsWith("/"))
                        {
                            appendUrl = appendUrl.Substring(1, appendUrl.Length - 1);
                        }
                        var redirectUrl = $"{parentUrl}{appendUrl}";
                        var validateUrl = WebUtility.GetRelativeUrl(redirectUrl);

                        return redirectUrl;
                        //var validateNode = queryUtil?.GetNodeByUrl(validateUrl);
                        //if (validateNode.NodeExists())
                        //{
                        //    return redirectUrl;
                        //}
                    }
                }
            }

            return "";
        }

        public bool HandleUrlChange(IPublishedContent? content, string note = "")
        {
            var createdRedirect = false;
            if (queryUtil == null || content == null || pcUtil == null || contentService == null || umbracoHelper == null) {
                return createdRedirect;
            }

            //first check if record exists
            var autoGeneratedFolder = queryUtil.GetPageByUniqueId(UmbracoCustomLookups.AutoGeneratedRedirects, UmbracoCustomTypes.RedirectFolder);
            if (autoGeneratedFolder != null)
            {
                var matchingNodes = pcUtil.FindAllNodes(autoGeneratedFolder, UmbracoCustomTypes.RedirectItem).Select(i => new { ContentNode = i, SourceNode = pcUtil.GetContentPicker(i, UmbracoCustomFields.SourceNode) })
                    //.Where(i => i.sourceNode != null && i.sourceNode.Id == content.Id)
                    .Where(i => WebUtility.GetRelativeUrl(content.Url()).Equals(WebUtility.GetRelativeUrl(pcUtil.GetContentValue(i.ContentNode, UmbracoCustomFields.OldUrl))))
                    .ToList();

                if (!matchingNodes?.Any() ?? false)
                {
                    //add new url redirect
                    //var redirectName = $"{content.Name} - {DateTime.Now.ToString("yyyy-MM-dd h:mm tt")}";
                    var oldUrl = WebUtility.GetRelativeUrl(content.Url());
                    var sourcePickerId = pcUtil.GetUdiString(umbracoHelper.Content(content.Id));
                    var redirectName = oldUrl;
                    var redirectContent = contentService.Create(redirectName, autoGeneratedFolder.Id, UmbracoCustomTypes.RedirectItem);
                    if (redirectContent != null)
                    {
                        redirectContent.SetValue(UmbracoCustomFields.OldUrl, oldUrl);
                        redirectContent.SetValue(UmbracoCustomFields.SourceNode, sourcePickerId);
                        redirectContent.SetValue(UmbracoCustomFields.History, note);
                        contentService.SaveAndPublish(redirectContent);
                        createdRedirect = true;
                    }
                }
                else
                {
                    //update existing record
                    var matchingNode = matchingNodes?.OrderByDescending(i => i.ContentNode.CreateDate).FirstOrDefault(i => pcUtil.NodeExists(i.ContentNode));
                    var removeNodes = matchingNodes?.Where(i => i.ContentNode.Id != matchingNode?.ContentNode.Id && pcUtil.NodeExists(i.ContentNode));

                    if (matchingNode != null)
                    {
                        var updateContent = contentService.GetById(matchingNode.ContentNode.Id);
                        if (updateContent != null)
                        {
                            var existingNote = contentUtil?.GetContentValue(updateContent, UmbracoCustomFields.History);
                            var updateNote = string.Empty;
                            if (!string.IsNullOrEmpty(existingNote))
                            {
                                updateNote = $"\n{existingNote}";
                            }
                            updateNote = $"{note}{updateNote}";

                            var oldUrl = WebUtility.GetRelativeUrl(content.Url());
                            var sourcePickerId = pcUtil.GetUdiString(umbracoHelper.Content(content.Id));
                            //var redirectName = $"{content.Name} - {DateTime.Now.ToString("yyyy-MM-dd h:mm tt")}";
                            var redirectName = oldUrl;

                            updateContent.Name = redirectName;
                            updateContent.SetValue(UmbracoCustomFields.OldUrl, oldUrl);
                            updateContent.SetValue(UmbracoCustomFields.SourceNode, sourcePickerId);
                            updateContent.SetValue(UmbracoCustomFields.History, updateNote);
                            contentService.SaveAndPublish(updateContent);

                            if (removeNodes != null)
                            {
                                foreach (var removeNode in removeNodes)
                                {
                                    //delete other duplicates
                                    var removeContent = contentService.GetById(removeNode.ContentNode.Id);
                                    if (removeContent != null) {
                                        contentService.Delete(removeContent);
                                    }
                                }
                            }

                            createdRedirect = true;
                        }
                    }
                }
            }

            return createdRedirect;
        }
    }
}

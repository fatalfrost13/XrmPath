using System;
using Umbraco.Core.Logging;

namespace XrmPath.Web.Helpers.UmbracoHelpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using RJP.MultiUrlPicker.Models;
    using Models;
    using Umbraco.Core.Models;
    using umbraco.interfaces;
    using XrmPath.Helpers.Utilities;
    using Utils;
    public static class MultiUrlUtility
    {
       
        public static UrlPicker GetUrlPicker(int nodeId, string alias = "urlPicker")
        {
            var content = ServiceUtility.UmbracoHelper.GetById(nodeId);
            return GetUrlPicker(content, alias);
        }

        public static UrlPicker GetUrlPicker(IPublishedContent content, string alias = "urlPicker")
        {
            var urlPicker = new UrlPicker();
            try
            {
                var stringData = content.GetContentValue(alias);
                var links = JsonConvert.DeserializeObject<JArray>(stringData);
                var firstLink = links?.FirstOrDefault();

                if (firstLink != null)
                {
                    var item = new Link(firstLink);
                    var url = item.Url ?? string.Empty;
                    if (url.StartsWith("/"))
                    {
                        url = SiteUrlUtility.GetSiteUrl(url);
                    }

                    urlPicker = new UrlPicker
                    {
                        Title = item.Name,
                        LinkType = item.Type,
                        NewWindow = item.Target == "_blank",
                        NodeId = item.GetIdFromLink(),
                        Url = url
                    };

                    if (string.IsNullOrEmpty(urlPicker.Url))
                    {
                        //URL Picker has a selected item but cannot find url
                        urlPicker.Url = "#";
                    }

                }

                if (string.IsNullOrEmpty(urlPicker.Title))
                {
                    urlPicker.Title = content.GetTitle();
                }

                if (urlPicker.Url == null)
                {
                    urlPicker.Url = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MultiUrlUtility.GetUrlPicker(IPublishedContent). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return urlPicker;
        }

        public static UrlPicker GetUrlPicker(IContent content, string alias = "urlPicker")
        {
            var urlPicker = new UrlPicker();
            try
            {
                var stringData = content.GetContentValue(alias);
                var links = JsonConvert.DeserializeObject<JArray>(stringData);
                var firstLink = links?.FirstOrDefault();

                if (firstLink != null)
                {
                    var item = new Link(firstLink);
                    var url = item.Url ?? string.Empty;

                    if (url.StartsWith("/"))
                    {
                        url = SiteUrlUtility.GetSiteUrl(url);
                    }

                    urlPicker = new UrlPicker
                    {
                        Title = item.Name,
                        LinkType = item.Type,
                        NewWindow = item.Target == "_blank",
                        NodeId = item.GetIdFromLink(),
                        Url = url
                    };
                }

                if (string.IsNullOrEmpty(urlPicker.Title))
                {
                    urlPicker.Title = content.GetTitle();
                }

                if (string.IsNullOrEmpty(urlPicker.Url))
                {
                    urlPicker.Url = "#";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MultiUrlUtility.GetUrlPicker(IContent). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return urlPicker;
        }

        public static List<UrlPicker> GetUrlPickerList(int nodeId, string alias)
        {
            var urlPickerList = new List<UrlPicker>();

            try
            {
                var node = ServiceUtility.UmbracoHelper.GetById(nodeId);
                var stringData = node.GetContentValue(alias);
                var links = JsonConvert.DeserializeObject<JArray>(stringData);

                if (!string.IsNullOrEmpty(stringData) && links.Any())
                {
                    foreach (var link in links)
                    {
                        var item = new Link(link);
                        var url = item.Url ?? string.Empty;

                        if (url.StartsWith("/"))
                        {
                            url = SiteUrlUtility.GetSiteUrl(url);
                        }

                        var urlPicker = new UrlPicker
                        {
                            Title = item.Name,
                            LinkType = item.Type,
                            NewWindow = item.Target == "_blank",
                            NodeId = item.GetIdFromLink(),
                            Url = url
                        };
                        urlPickerList.Add(urlPicker);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MultiUrlUtility.GetUrlPickerList. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return urlPickerList;
        }

        public static List<UrlPicker> GetUrlPickerList(string jsonData)
        {
            var urlPickerList = new List<UrlPicker>();
            try
            {
                var stringData = jsonData;
                var links = JsonConvert.DeserializeObject<JArray>(stringData);

                if (!string.IsNullOrEmpty(stringData) && links.Any())
                {
                    foreach (var link in links)
                    {
                        var item = new Link(link);
                        var url = item.Url ?? string.Empty;

                        if (url.StartsWith("/"))
                        {
                            url = SiteUrlUtility.GetSiteUrl(url);
                        }

                        var urlPicker = new UrlPicker
                        {
                            Title = item.Name,
                            LinkType = item.Type,
                            NewWindow = item.Target == "_blank",
                            NodeId = item.GetIdFromLink(),
                            Url = url
                        };

                        urlPickerList.Add(urlPicker);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MultiUrlUtility.GetUrlPickerList. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return urlPickerList;
        }

        public static string MultiUrlToString(List<UrlPicker> multiUrlPicker)
        {
            var jsonData = "";

            try
            {
                foreach (var url in multiUrlPicker)
                {
                    var icon = "icon-link";
                    var linkItem = "{";

                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        linkItem = ", {";
                    }

                    if (url.NodeId > 0)
                    {
                        linkItem += "\"id\": \"" + url.NodeId + "\",";
                    }

                    if (url.LinkType == LinkType.Content && (url.NodeId != null && url.NodeId > 0))
                    {
                        var content = ServiceUtility.UmbracoHelper.GetById((int) url.NodeId);
                        linkItem += "\"name\": " + content.GetTitle();
                        icon = ServiceUtility.ContentTypeService.GetAllContentTypes().First(i => i.Alias == content.DocumentTypeAlias).Icon;
                        if (string.IsNullOrEmpty(url.Url) || url.Url == "#")
                        {
                            url.Url = content.GetUrl();
                        }
                    }
                    else if (url.LinkType == LinkType.Media && (url.NodeId != null && url.NodeId > 0))
                    {
                        var mediaId = (int) url.NodeId;
                        var media = MediaUtility.GetMediaItem(mediaId);
                        var filePath = media.Url;
                        linkItem += "\"name\": " + media.Name;
                        linkItem += "\"isMedia\": true,";
                        if (string.IsNullOrEmpty(url.Url) || url.Url == "#")
                        {
                            url.Url = filePath;
                            //url.Url = MediaUtility.GetMediaPath(mediaId);
                        }

                        url.NewWindow = true;
                        icon = "icon-document";
                        if (filePath.EndsWith(".jpg") || filePath.EndsWith(".gif") || filePath.EndsWith(".tif") || filePath.EndsWith(".png") || filePath.EndsWith(".bmp"))
                        {
                            icon = "icon-picture";
                        }
                    }
                    else
                    {
                        linkItem += "\"name\": \"" + url.Url + "\",";
                    }

                    if (url.NewWindow)
                    {
                        linkItem += "\"target\": \"" + "_blank" + "\",";
                    }
                    else
                    {
                        linkItem += "\"target\": \"" + "\",";
                    }

                    if (!string.IsNullOrEmpty(url.Url))
                    {
                        linkItem += "\"url\": \"" + url.Url + "\",";
                    }

                    linkItem += "\"icon\": \"" + icon + "\"";
                    linkItem += "}";

                    jsonData += linkItem;
                }
                jsonData = $"[{jsonData}]";
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MultiUrlUtility.MultiUrlToString. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return jsonData;
        }

        public static string UrlPickerLink(string jSonUrlPicker, INode navNode, string urlPickerAlias, string property = "")
        {
            var strTitle = "";
            var strTarget = "";

            var navTitle = navNode.GetProperty("pageTitle");
            if (navTitle != null)
            {
                strTitle = navTitle.Value;
            }

            //Array arr = csvURLPicker.Split(',');
            var urlPicker = GetUrlPicker(navNode.Id, urlPickerAlias);

            //0 = Link Type
            //1 = Open new window
            //2 = node ID if applicable
            //3 = link url
            //4 = link title

            var newWindow = urlPicker.NewWindow;

            if (newWindow)
            {
                strTarget = " target=\"_blank\"";
            }

            var strUrl = urlPicker.Url ?? "#";

            if (strUrl.StartsWith("/"))
            {
                strUrl = SiteUrlUtility.GetSiteUrl(strUrl);
            }

            if (!string.IsNullOrEmpty(urlPicker.Title))
            {
                strTitle = urlPicker.Title;
            }

            var strLink = $"<a href=\"{strUrl}\"{strTarget}>{strTitle}</a>";


            if (property != "")
            {
                switch (property)
                {
                    case "Url":
                        strLink = strUrl;
                        break;
                    case "Title":
                        strLink = strTitle;
                        break;
                    case "Target":
                        strLink = strTarget;
                        break;
                }
            }

            return strLink;
        }

        public static string UrlPickerLink(string jSonUrlPicker, IPublishedContent navContent, string urlPickerAlias, string property = "")
        {
            var strTitle = "";
            var strTarget = "";

            var navTitle = navContent.GetProperty("pageTitle");
            if (navTitle != null)
            {
                strTitle = navTitle.Value.ToString();
            }

            //Array arr = csvURLPicker.Split(',');
            var urlPicker = GetUrlPicker(navContent.Id, urlPickerAlias);

            //0 = Link Type
            //1 = Open new window
            //2 = node ID if applicable
            //3 = link url
            //4 = link title

            var newWindow = urlPicker.NewWindow;

            if (newWindow)
            {
                strTarget = " target=\"_blank\"";
            }

            var strUrl = urlPicker.Url ?? "#";

            if (strUrl.StartsWith("/"))
            {
                strUrl = SiteUrlUtility.GetSiteUrl(strUrl);
            }

            if (!string.IsNullOrEmpty(urlPicker.Title))
            {
                strTitle = urlPicker.Title;
            }

            var strLink = $"<a href=\"{strUrl}\"{strTarget}>{strTitle}</a>";


            if (property != "")
            {
                switch (property)
                {
                    case "Url":
                        strLink = strUrl;
                        break;
                    case "Title":
                        strLink = strTitle;
                        break;
                    case "Target":
                        strLink = strTarget;
                        break;
                }
            }

            return strLink;
        }
    }
}
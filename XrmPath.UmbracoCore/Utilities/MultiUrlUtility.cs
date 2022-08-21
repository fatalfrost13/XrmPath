using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using XrmPath.Helpers.Utilities;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;
using XrmPath.UmbracoCore.Models;
using XrmPath.UmbracoCore.Helpers;

namespace XrmPath.UmbracoCore.Utilities
{

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
   
                //var stringData = content.GetContentValue(alias);
                //var links = JsonConvert.DeserializeObject<JArray>(stringData);
                //var firstLink = links?.FirstOrDefault();

                Link firstLink = null;
                var stringData = content.GetContentValue(alias);
                var links = new List<Link>();
                if (!string.IsNullOrEmpty(stringData))
                {
                    var obj = content.GetProperty(alias).GetValue();
                    if (obj.GetType() == typeof(Link))
                    {
                        firstLink = (Link)obj;
                    }
                    else 
                    {
                        links = (List<Link>)obj;
                        if (links.Any())
                        {
                            firstLink = links.FirstOrDefault();
                        }
                    }

                }

                if (firstLink != null)
                {
                    //var item = new Link(firstLink);
                    //var item = JsonConvert.DeserializeObject<Link>(stringData);
                    var url = firstLink.Url ?? string.Empty;
                    if (url.StartsWith("/"))
                    {
                        url = SiteUrlHelper.GetSiteUrl(url);
                    }

                    urlPicker = new UrlPicker
                    {
                        Title = firstLink.Name,
                        LinkType = firstLink.Type,
                        NewWindow = firstLink.Target == "_blank",
                        NodeId = firstLink.GetIdFromLink(firstLink.Type),
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
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MultiUrlUtility.GetUrlPicker(IPublishedContent). URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MultiUrlUtility.GetUrlPicker(IPublishedContent). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return urlPicker;
        }

        public static UrlPicker GetUrlPicker(IContent content, string alias = "urlPicker")
        {
            var urlPicker = new UrlPicker();
            try
            {

                var stringData = content.GetContentValue(alias);
                //var links = JsonConvert.DeserializeObject<JArray>(stringData);
                //var firstLink = links?.FirstOrDefault();

                Link firstLink = null;
                var links = new List<Link>();
                if (!string.IsNullOrEmpty(stringData))
                {
                    //links = (List<Link>)node.GetProperty(alias).GetValue();
                    links = (List<Link>)content.GetValue(alias);
                    if (links.Any()) 
                    {
                        firstLink = links.FirstOrDefault();
                    }

                    var obj = content.GetValue(alias);
                    if (obj.GetType() == typeof(Link))
                    {
                        firstLink = (Link)obj;
                    }
                    else
                    {
                        links = (List<Link>)obj;
                        if (links.Any())
                        {
                            firstLink = links.FirstOrDefault();
                        }
                    } 
                }
                

                if (firstLink != null)
                {
                    //var item = new Link(firstLink);
                    var item = JsonConvert.DeserializeObject<Link>(stringData);
                    var url = item.Url ?? string.Empty;

                    if (url.StartsWith("/"))
                    {
                        url = SiteUrlHelper.GetSiteUrl(url);
                    }

                    urlPicker = new UrlPicker
                    {
                        Title = item.Name,
                        LinkType = item.Type,
                        NewWindow = item.Target == "_blank",
                        NodeId = item.GetIdFromLink(item.Type),
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
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MultiUrlUtility.GetUrlPicker(IContent). URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MultiUrlUtility.GetUrlPicker(IContent). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return urlPicker;
        }

        public static List<UrlPicker> GetUrlPickerList(int nodeId, string alias)
        {
            var urlPickerList = new List<UrlPicker>();

            try
            {
                var content = ServiceUtility.UmbracoHelper.GetById(nodeId);
                var stringData = content.GetContentValue(alias);
                var links = new List<Link>();

                if (!string.IsNullOrEmpty(stringData)) 
                {
                    //links = (List<Link>)node.GetProperty(alias).GetValue();
                    var obj = content.GetProperty(alias).GetValue();
                    if (obj.GetType() == typeof(Link))
                    {
                        links.Add((Link)obj);
                    }
                    else
                    {
                        links = (List<Link>)obj;
                    }
                }

                if (links.Any())
                {
                    foreach (var link in links)
                    {
                        //var item = new Link(link);
                        //var item = JsonConvert.DeserializeObject<Link>(stringData);
                        var url = link.Url ?? string.Empty;

                        if (url.StartsWith("/"))
                        {
                            url = SiteUrlHelper.GetSiteUrl(url);
                        }

                        var urlPicker = new UrlPicker
                        {
                            Title = link.Name,
                            LinkType = link.Type,
                            NewWindow = link.Target == "_blank",
                            NodeId = link.GetIdFromLink(link.Type),
                            Url = url
                        };
                        urlPickerList.Add(urlPicker);
                    }
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MultiUrlUtility.GetUrlPickerList. URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MultiUrlUtility.GetUrlPickerList. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return urlPickerList;
        }


        public static string UrlPickerLink(IPublishedContent navContent, string urlPickerAlias, string property = "")
        {
            var strTitle = "";
            var strTarget = "";

            var navTitle = navContent.GetProperty("pageTitle");
            if (navTitle != null)
            {
                strTitle = navTitle.GetValue().ToString();
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
                strUrl = SiteUrlHelper.GetSiteUrl(strUrl);
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
using Microsoft.IdentityModel.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using XrmPath.UmbracoUtils.Models;


namespace XrmPath.UmbracoUtils
{
    public class MultiUrlUtility
    {
        private readonly PublishedContentUtility _pcUtil;
        private readonly UmbracoHelper? _umbracoHelper;
        public MultiUrlUtility(PublishedContentUtility pcUtil) { 
            _pcUtil = pcUtil;
            _umbracoHelper = _pcUtil.GetUmbracoHelper();
        }

        public UrlPicker GetUrlPicker(int nodeId, string alias = "urlPicker")
        {
            var content = _umbracoHelper?.Content(nodeId);
            return GetUrlPicker(content, alias);
        }

        public UrlPicker GetUrlPicker(IPublishedContent? content, string alias = "urlPicker")
        {
            var urlPicker = new UrlPicker();
            try
            {
                Link? firstLink = null;
                var stringData = _pcUtil.GetContentValue(content, alias);
                var links = new List<Link>();
                if (content != null && !string.IsNullOrEmpty(stringData))
                {
                    var obj = content.GetProperty(alias)?.GetValue();
                    if (obj?.GetType() == typeof(Link))
                    {
                        firstLink = (Link)obj;
                    }
                    else if (obj != null)
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
                    var url = firstLink.Url ?? string.Empty;
                    //if (url.StartsWith("/"))
                    //{
                    //    url = SiteUrlHelper.GetSiteUrl(url);
                    //}

                    urlPicker = new UrlPicker
                    {
                        Title = firstLink?.Name ?? "",
                        LinkType = firstLink?.Type ?? LinkType.Content,
                        NewWindow = firstLink?.Target == "_blank",
                        NodeId = GetIdFromLink(firstLink),
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
                    urlPicker.Title = _pcUtil.GetTitle(content);
                }

                if (urlPicker.Url == null)
                {
                    urlPicker.Url = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MultiUrlUtility.GetUrlPicker(IPublishedContent). URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on MultiUrlUtility.GetUrlPicker(IPublishedContent). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return urlPicker;
        }

        public string UrlPickerLink(IPublishedContent? navContent, string urlPickerAlias, string property = "")
        {
            var strTitle = "";
            var strTarget = "";

            var navTitle = navContent?.GetProperty(UmbracoCustomFields.NavigationTitle);
            if (navTitle != null)
            {
                strTitle = navTitle?.GetValue()?.ToString() ?? "";
            }

            //Array arr = csvURLPicker.Split(',');
            var urlPicker = navContent != null ? GetUrlPicker(navContent.Id, urlPickerAlias) : new UrlPicker();

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

            //if (strUrl.StartsWith("/"))
            //{
            //    strUrl = SiteUrlHelper.GetSiteUrl(strUrl);
            //}

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

        public int GetIdFromLink(Link? item)
        {
            //var nodeId = item?.Id ?? 0;

            var nodeId = 0;
            try
            {
                if (item?.Udi != null)
                {
                    if (item.Type == LinkType.Content)
                    {

                        var node = _umbracoHelper?.Content(item.Udi);
                        if (node != null && _pcUtil.NodeExists(node))
                        {
                            nodeId = node.Id;
                        }
                    }
                    else if (item.Type == LinkType.Media)
                    {
                        var node = _umbracoHelper?.Media(item.Udi);
                        if (node != null && _pcUtil.NodeExists(node))
                        {
                            nodeId = node.Id;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetIdFromLink(). URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetIdFromLink(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return nodeId;
        }
    }
}

using Microsoft.IdentityModel.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using XrmPath.Helpers.Utilities;
using XrmPath.Web.UmbracoUtils;

namespace XrmPath.UmbracoUtils
{
    public class PublishedContentUtility: BaseUtility
    {

        private readonly MultiUrlUtility _urlUtil;
        public PublishedContentUtility(UmbracoHelper? umbracoHelper = null, IMediaService? mediaService = null) : base(umbracoHelper, mediaService)
        {
            _urlUtil = new MultiUrlUtility(this);
        }

        public bool NodeExists(IPublishedContent? content)
        {
            if (content != null && content.Id > 0)
            {
                return true;
            }
            return false;
        }

        public string GetContentValue(IPublishedContent? content, string propertyAlias, string defaultValue = "")
        {
            var result = defaultValue;
            if (string.IsNullOrEmpty(propertyAlias))
            {
                return result;
            }
            try
            {
                if (NodeExists(content))
                {
                    var property = content.GetProperty(propertyAlias);
                    if (property != null && property.HasValue() && !string.IsNullOrEmpty(property.GetValue()?.ToString()))
                    {
                        result = property.GetValue()?.ToString();
                    }

                    //result = TemplateUtilities.ParseInternalLinks(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentValue() for DocumentTypeAlias: {propertyAlias}. URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentValue() for DocumentTypeAlias: {propertyAlias}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return result ?? String.Empty;
        }

        public string GetUrl(IPublishedContent content, string alias = "urlPicker")
        {
            var strUrl = "";
            if (this.NodeExists(content))
            {
                //var reverseProxyFolder = SiteUrlUtility.GetRootFromReverseProxy();
                var nodeUrl = content.Url();
                strUrl = nodeUrl;

                if (!string.IsNullOrEmpty(alias))
                {
                    //check URL Property
                    var stringData = this.GetContentValue(content, alias);
                    if (!string.IsNullOrWhiteSpace(stringData))
                    {
                        strUrl = _urlUtil.UrlPickerLink(content, alias, "Url");
                        if (string.IsNullOrWhiteSpace(strUrl))
                        {
                            strUrl = nodeUrl;
                        }
                    }
                }
            }
            return strUrl;
        }

        public string GetTarget(IPublishedContent content, string alias = "urlPicker")
        {
            string strTarget = "_self";
            if (this.NodeExists(content))
            {
                //check URL Property
                var stringData = this.GetContentValue(content, alias);
                if (!string.IsNullOrWhiteSpace(stringData))
                {
                    strTarget = _urlUtil.UrlPickerLink(content, alias, "Target");
                    if (strTarget.Trim().Contains("_blank"))
                    {
                        strTarget = "_blank";
                    }
                    else if (strTarget.Trim() == "")
                    {
                        strTarget = "_self";
                    }
                }
            }

            return strTarget;
        }
        public string GetTitle(IPublishedContent? content, string aliases = "title,pageTitle,name")
        {
            var strTitle = string.Empty;
            if (content != null)
            {
                strTitle = content.Name;
                if (aliases.Contains(","))
                {
                    var aliasList = aliases.StringToSet();
                    foreach (var alias in aliasList)
                    {
                        var title = this.GetContentValue(content, alias);
                        if (!string.IsNullOrEmpty(title))
                        {
                            return title;
                        }
                    }
                }
                else
                {
                    strTitle = this.GetContentValue(content, aliases);
                }
            }
            return strTitle ?? "";
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
                        if (node != null && this.NodeExists(node))
                        {
                            nodeId = node.Id;
                        }
                    }
                    else if (item.Type == LinkType.Media)
                    {
                        var node = _umbracoHelper?.Media(item.Udi);
                        if (node != null && this.NodeExists(node))
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

        public IPublishedContent? GetContentById(int nodeId)
        {
            var content = _umbracoHelper?.Content(nodeId);
            return content;
        }

    }
}

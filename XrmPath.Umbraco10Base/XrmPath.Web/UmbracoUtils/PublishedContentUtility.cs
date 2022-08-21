using Umbraco.Cms.Core.Models.PublishedContent;

namespace XrmPath.UmbracoUtils
{
    public static class PublishedContentUtility
    {
        public static bool NodeExists(this IPublishedContent content)
        {
            if (content != null && content.Id > 0)
            {
                return true;
            }
            return false;
        }

        public static string GetContentValue(this IPublishedContent content, string propertyAlias, string defaultValue = "")
        {
            var result = defaultValue;
            if (string.IsNullOrEmpty(propertyAlias))
            {
                return result;
            }
            try
            {
                if (content.NodeExists())
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

        public static string GetUrl(this IPublishedContent content, string alias = "urlPicker")
        {
            var strUrl = "";
            if (content.NodeExists())
            {
                //var reverseProxyFolder = SiteUrlUtility.GetRootFromReverseProxy();
                var nodeUrl = content.Url();
                strUrl = nodeUrl;

                if (!string.IsNullOrEmpty(alias))
                {
                    //check URL Property
                    var stringData = content.GetContentValue(alias);
                    if (!string.IsNullOrWhiteSpace(stringData))
                    {
                        //strUrl = MultiUrlUtility.UrlPickerLink(content, alias, "Url");
                        strUrl = "";
                        if (string.IsNullOrWhiteSpace(strUrl))
                        {
                            strUrl = nodeUrl;
                        }
                    }
                }
            }
            return strUrl;
        }

        public static string GetTarget(this IPublishedContent content, string alias = "urlPicker")
        {
            string strTarget = "_self";
            if (content.NodeExists())
            {
                //check URL Property
                var stringData = content.GetContentValue(alias);
                if (!string.IsNullOrWhiteSpace(stringData))
                {
                    //strTarget = MultiUrlUtility.UrlPickerLink(content, alias, "Target");
                    strTarget = "_self";
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
    }
}

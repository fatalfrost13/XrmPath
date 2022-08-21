using XrmPath.Helpers.Utilities;
using System;
using System.Configuration;
using System.Linq;
using System.Web;

namespace XrmPath.UmbracoCore.Helpers
{
    public static class SiteUrlHelper
    {

        /// <summary>
        /// Used to set URL of internal image path. 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetSiteUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                //run under sub folder from configuration
                var configurationRoot = GetRootFromConfiguration();
                if (!string.IsNullOrEmpty(configurationRoot))
                {
                    if ((url.IndexOf("http://", StringComparison.Ordinal) == -1 && url.IndexOf("https://", StringComparison.Ordinal) == -1) || url.StartsWith("/"))
                    {
                        //link is relative
                        var subFolderLink = $"{configurationRoot}{url}";
                        return subFolderLink;
                    }
                }
            }
            return url;
        }
        public static string GetFullUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            { 
                if ((url.IndexOf("http://", StringComparison.Ordinal) == -1 && url.IndexOf("https://", StringComparison.Ordinal) == -1) || url.StartsWith("/"))
                {
                    //http does not exists, append
                    var domain = WebUtility.GetDomain();
                    var fullUrl = $"{domain}{url}";
                    return fullUrl;
                }
            }
            return url;
        }

        /// <summary>
        /// Get Root path from app key setting in web.config
        /// This will also isolate media and images link into the specified sub folder
        /// </summary>
        /// <returns></returns>
        public static string GetRootFromConfiguration()
        {
            var subFolderRoot = ConfigurationManager.AppSettings["umbracoRootSubfolder"];
            if (!string.IsNullOrEmpty(subFolderRoot) && subFolderRoot.StartsWith("/"))
            {
                if (subFolderRoot.EndsWith("/"))
                {
                    subFolderRoot = subFolderRoot.Substring(0, subFolderRoot.Length - 1);
                }
                return subFolderRoot;
            }
            return string.Empty;
        }
    }
}
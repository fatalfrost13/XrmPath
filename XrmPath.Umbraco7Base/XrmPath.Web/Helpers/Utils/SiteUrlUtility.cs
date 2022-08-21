using XrmPath.Helpers.Utilities;
using System;
using System.Configuration;
using System.Linq;
using System.Web;

namespace XrmPath.Web.Helpers.Utils
{
    public static class SiteUrlUtility
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

                //run under sub folder from reverse proxy
                //var reverseProxyRoot = GetRootFromReverseProxy();
                //if (!string.IsNullOrEmpty(reverseProxyRoot))
                //{
                //    if ((url.IndexOf("http://", StringComparison.Ordinal) == -1 && url.IndexOf("https://", StringComparison.Ordinal) == -1) || url.StartsWith("/"))
                //    {
                //        //link is relative
                //        var subFolderLink = $"{reverseProxyRoot}{url}";
                //        return subFolderLink;
                //    }
                //}

                //run under sub folder from configuration
                //var configurationRoot = GetRootFromConfiguration();
                //if(!string.IsNullOrEmpty(configurationRoot))
                //{
                //    if ((url.IndexOf("http://", StringComparison.Ordinal) == -1 && url.IndexOf("https://", StringComparison.Ordinal) == -1) || url.StartsWith("/"))
                //    {
                //        //link is relative
                //        var subFolderLink = $"{configurationRoot}{url}";
                //        return subFolderLink;
                //    }
                //}
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
        /// Set URL of Script and CSS paths
        /// </summary>
        /// <param name="valueIfBlank"></param>
        /// <returns></returns>
        public static string RootUrl(string valueIfBlank = "")
        {
            //run under sub folder proxy
            //check first if the header passes in a relative path for re-mapping
            var reverseProxyRoot = GetRootFromReverseProxy();
            var configurationRoot = GetRootFromConfiguration();

            if (!string.IsNullOrEmpty(reverseProxyRoot))
            {
                return reverseProxyRoot;
            }

            //run under assigned domain url
            var primaryDomain = ConfigurationManager.AppSettings["umbracoPrimaryDomain"];
            if (!string.IsNullOrEmpty(primaryDomain))
            { 
                var domainUrl = WebUtility.GetDomain(primaryDomain);
                if (!string.IsNullOrEmpty(domainUrl))
                {
                    //var domainCurrent = WebUtility.GetDomain();
                    //if (!domainUrl.ToLower().Equals(domainCurrent.ToLower()))
                    //{
                    //    //only use absolute paths if domain registered in web.config does not match current domain
                    //    var domain = !string.IsNullOrEmpty(domainUrl) ? domainUrl : domainCurrent;
                    //    return domain;
                    //}
                    
                    if (!string.IsNullOrEmpty(configurationRoot))
                    {
                        domainUrl = $"{domainUrl}{configurationRoot}";
                    }

                    return domainUrl;
                }
            }

            if (!string.IsNullOrEmpty(configurationRoot))
            {
                return configurationRoot;
            }

            return valueIfBlank;
        }

        /// <summary>
        /// Gets the Root path of site from reverse proxy
        /// </summary>
        /// <returns></returns>
        public static string GetRootFromReverseProxy()
        {
            //"X-Original-URL" outputs "/export-tool"
            //can also accept a value with full domain, ex: "https://uat.alberta.ca/export-tool"
            var subFoldersFromHeader = HttpContext.Current?.Request.Headers["X-Original-URL"] ?? string.Empty;
            if (!string.IsNullOrEmpty(subFoldersFromHeader))
            {
                if(subFoldersFromHeader.Contains("http://") || subFoldersFromHeader.Contains("https://"))
                {
                    //remove domain from the string
                    //in case the variable contains full url (ex. https://uat.alberta.ca/export-tool)
                    var domain = WebUtility.GetDomain(subFoldersFromHeader);
                    subFoldersFromHeader = subFoldersFromHeader.Replace(domain,"");
                    if (subFoldersFromHeader.EndsWith("/"))
                    {
                        subFoldersFromHeader = subFoldersFromHeader.Substring(0, subFoldersFromHeader.Length - 1);
                    }
                }

                var firstFolder = subFoldersFromHeader.Split('/').First(i => !string.IsNullOrEmpty(i));
                if (firstFolder.Contains("?"))
                {
                    firstFolder = firstFolder.Split('?').FirstOrDefault();
                }
                var rootFolder = $"/{firstFolder}";
                return rootFolder;
            }
            return string.Empty;
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
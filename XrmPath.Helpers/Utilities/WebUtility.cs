using System;
using System.Web;

namespace XrmPath.Helpers.Utilities
{
    public static class WebUtility
    {
        /// <summary>
        /// Get Domain of the requested url.
        /// </summary>
        /// <param name="url">Pass in null to request the current domain.</param>
        /// <returns></returns>
        public static string GetDomain(string url = null)
        {
            var originalUrl = HttpContext.Current.Request.Url;

            if ((url == null || url == "#") && originalUrl != null)
            {
                //get current domain
                //url parameter is null, so the current domain is being requested
                url = originalUrl.AbsoluteUri;
            }

            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            } 

            if (!url.Contains("http://") && !url.Contains("https://") && !url.StartsWith("/"))
            {
                url = originalUrl.AbsoluteUri.Contains("https://") ? $"https://{url}" : $"http://{url}";
            }
            else if(url.StartsWith("/"))
            {
                var pathQuery = originalUrl.PathAndQuery;
                var hostName = originalUrl.ToString().Replace(pathQuery, "");
                return hostName;
            }

            if (!string.IsNullOrEmpty(url))
            {
                originalUrl = new Uri(url);
            }

            var domain = originalUrl.Host;
            var absoluteUri = originalUrl.AbsoluteUri.Trim();
            domain = absoluteUri.StartsWith("https://") ? $"https://{domain}" : $"http://{domain}";

            if (originalUrl.Host.Equals("localhost"))
            {
                //append port number
                domain = $"{domain}:{originalUrl.Port}";
            }
            
            return domain;
        }

        public static string GetApiReferrerDomain()
        {
            var currentUrlRequest = HttpContext.Current?.Request.Url.ToString() ?? string.Empty;
            if (currentUrlRequest.Contains("/api/"))
            {
                var referrer = HttpContext.Current?.Request.UrlReferrer?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(referrer))
                {
                    var referrerUrl = new Uri(referrer);
                    var referrerDomain = GetDomain(referrerUrl.AbsoluteUri);
                    return referrerDomain;
                }
            }
            return null;
        }

        public static string GetRelativeUrl(string url)
        {
            var relativeUrl = url;
            var domain = GetDomain(url);
            relativeUrl = relativeUrl.Replace($"{domain}", "");
            
            //remove querystring
            if (relativeUrl.Contains("?"))
            {
                relativeUrl = relativeUrl.Substring(0, relativeUrl.LastIndexOf("?"));
            }

            //remove trailing backslash
            if (relativeUrl.EndsWith("/"))
            {
                relativeUrl = relativeUrl.Substring(0, relativeUrl.LastIndexOf("/"));
            }

            relativeUrl = relativeUrl.ToLower();

            return relativeUrl;
        }
    }
}
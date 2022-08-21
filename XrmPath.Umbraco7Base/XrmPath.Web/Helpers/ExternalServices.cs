using XrmPath.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Umbraco.Core.Logging;

namespace XrmPath.Web.Helpers
{
    public static class ExternalServices
    {

        public static ExternalWebContent GetExternalWebContent(string url)
        {
            string webData = string.Empty;

            if (!string.IsNullOrEmpty(url) )
            { 

                System.Net.WebClient wc = new System.Net.WebClient();
                byte[] raw = wc.DownloadData(url);
            
                try
                {
                    webData = System.Text.Encoding.UTF8.GetString(raw);
                }
                catch (Exception ex)
                {
                    LogHelper.Warn<string>($"XrmPath.Web caught error on ExternalServices.GetExternalWebContent(): Cannot get data from url({url}) Error: {ex}");
                }

            }

            var externalWebContent = new ExternalWebContent();
            externalWebContent.Url = url;
            externalWebContent.WebContent = webData;

            if (string.IsNullOrEmpty(webData)) {
                externalWebContent = null;
            }

            return externalWebContent;
        }
        public static ExternalWebContent GetExternalWebContent()
        {
            var url = string.Empty;
            if (string.IsNullOrEmpty(url))
            {
                url = HttpContext.Current.Request.QueryString["url"];
            }

            if (!string.IsNullOrEmpty(url))
            {
                var webData = GetExternalWebContent(url);
                if (!string.IsNullOrEmpty(webData?.WebContent))
                {
                    return webData;
                }
            }

            return null;
        }
    }
}
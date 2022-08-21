using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace XrmPath.Helpers.Utilities
{
    public static class UrlUtility
    {
        public static string FileUrlToDomain(this string originalUrl)
        {
            var newUrl = originalUrl;

            try
            {
                var currentContext = HttpContext.Current;

                if (currentContext == null)
                {
                    return originalUrl;
                }


                var urlBuilder = new UriBuilder(originalUrl);
                if (urlBuilder.Uri.ToString().UrlIsFile())
                {
                    urlBuilder.Host = currentContext.Request.Url.Host.ToLower();
                    newUrl = urlBuilder.Uri.ToString();
                }
                if (currentContext.Request.Url.ToString().IndexOf("https://", StringComparison.Ordinal) > -1)
                {
                    newUrl = newUrl.Replace("http://", "https://");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "XrmPath caught error on UrlHelper.FileUrlToDomain()");
                //LogHelper.Error<string>("XrmPath caught error on UrlHelper.FileUrlToDomain()", ex);
            }
            return newUrl;
        }

        public static bool UrlIsFile(this string originalUrl)
        {
            var isFile = false;
            try
            {
                var extention = VirtualPathUtility.GetExtension(originalUrl);
                var fileTypes = ".pdf,.doc,.docx,.ppt,.pptx,.xls,.xlsx,.txt,.jpg,.png,.gif,.mp3,.mp4,.wav,.csv";
                var fileTypesList = fileTypes.Split(',').ToList();

                if (extention == string.Empty)
                {
                    return false;
                }

                if (fileTypesList.Contains(extention))
                {
                    isFile = true;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "XrmPath caught error on UrlHelper.UrlIsFile()");
                //LogHelper.Error<bool>("XrmPath caught error on UrlHelper.UrlIsFile()", ex);
            }
            return isFile;
        }

        public static string UploadFilePath(string folderPath, string fileName, bool relativePath = true)
        {
            var fileSavePath = string.Empty;
            try
            {
                if (folderPath.StartsWith("~/") || folderPath.StartsWith("/") || folderPath.StartsWith(".."))
                {
                    if (relativePath)
                    {
                        fileSavePath = $"{folderPath}/{fileName}";
                        fileSavePath = fileSavePath.Replace("~/", "/");
                    }
                    else
                    {
                        // Get the complete file path
                        fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath(folderPath), fileName);
                    }
                }
                else
                {
                    if (!folderPath.EndsWith("\\"))
                    {
                        folderPath = $"{folderPath}\\";
                    }
                    fileSavePath = Path.Combine(folderPath, fileName);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "XrmPath caught error on UrlHelper.UploadFilePath()");
                //LogHelper.Error<string>("XrmPath caught error on UrlHelper.UploadFilePath()", ex);
            }
            return fileSavePath;
        }

        public static string GetDocumentLoaderLink(string fileName)
        {
            string documentLoaderUrl = null;
            try
            {
                var folderPath = ConfigurationManager.AppSettings["UploadedFilesPath"];
                var fullPath = UploadFilePath(folderPath, fileName, false);

                if ((!fullPath.StartsWith("~/") && !fullPath.StartsWith("/") && !fullPath.StartsWith("..")) &&
                    File.Exists(fullPath))
                {
                    documentLoaderUrl = $"/handlers/UploadedDocumentLoader.ashx?file={fileName}";
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "XrmPath caught error on UrlHelper.GetDocumentLoaderLink()");
                //LogHelper.Error<string>("XrmPath caught error on UrlHelper.GetDocumentLoaderLink()", ex);
            }
            return documentLoaderUrl;
        }

        public static bool UploadFileExists(string folderPath, string fileName)
        {
            var fileExists = false;
            try
            {
                var fullPath = UploadFilePath(folderPath, fileName);
                if ((!folderPath.StartsWith("~/") && !folderPath.StartsWith("/") && !folderPath.StartsWith("..")) &&
                    File.Exists(fullPath))
                {
                    fileExists = true;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "XrmPath caught error on UrlHelper.UploadFileExists()");
                //LogHelper.Error<bool>("XrmPath caught error on UrlHelper.UploadFileExists()", ex);
            }
            return fileExists;
        }

        //public static string UrlDisplay(this string url)
        //{
        //    var returnString = string.Empty;
        //    var listAddress= url.Split('-', ',').ForEach(x => x.ToFirstUpper()).ToList();
        //    listAddress.ForEach(x => { returnString += (x.ToString()+" "); });
        //    return returnString;
        //}

        public static string SecureUrlTransform(this string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                //relative link or not http request. just return original url
                return url;
            }

            var secureUrl = url;
            var currentUrl = HttpContext.Current?.Request.Url.ToString();
            if (currentUrl != null && currentUrl.StartsWith("https://"))
            {
                secureUrl = url.Replace("http://", "https://");
            }

            return secureUrl;
        }

        public static string GetCurrentUrl(string defaultUrl = "Unknown")
        {
            string currentUrl;
            try
            {
                currentUrl = HttpContext.Current?.Request.Url.AbsoluteUri ?? defaultUrl;
            }
            catch
            {
                currentUrl = defaultUrl;
            }
            return currentUrl;
        }
    }
}
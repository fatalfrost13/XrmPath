using Microsoft.AspNetCore.Http.Extensions;
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
                //var currentContext = HttpContext.Current;

                //if (currentContext == null)
                //{
                //    return originalUrl;
                //}

                var currentContext = HttpHelper.HttpContext;
                if (HttpHelper.HttpContext == null) {
                    return originalUrl;
                }
                HttpHelper.HttpContext.Request.GetDisplayUrl();
                var urlBuilder = new UriBuilder(originalUrl);
                if (urlBuilder.Uri.ToString().UrlIsFile())
                {
                    urlBuilder.Host = currentContext?.Request.Host.ToString().ToLower();
                    newUrl = urlBuilder.Uri.ToString();
                }
                if (currentContext?.Request.GetDisplayUrl().IndexOf("https://", StringComparison.Ordinal) > -1)
                {
                    newUrl = newUrl.Replace("http://", "https://");
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, "XrmPath caught error on UrlHelper.FileUrlToDomain()");
                Console.WriteLine(ex.Message);
            }
            return newUrl;
        }

        public static bool UrlIsFile(this string originalUrl)
        {
            var isFile = false;
            try
            {
                var extention = Path.GetExtension(originalUrl);
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
                //Serilog.Log.Error(ex, "XrmPath caught error on UrlHelper.UrlIsFile()");
                Console.WriteLine(ex.Message);
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
                        //fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath(folderPath), fileName);
                        fileSavePath = Path.Combine(HttpHelper.HttpContext?.Request?.PathBase ?? "", folderPath, fileName);
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
                //Serilog.Log.Error(ex, "XrmPath caught error on UrlHelper.UploadFilePath()");
                Console.WriteLine(ex.Message);
            }
            return fileSavePath;
        }

        public static string? GetDocumentLoaderLink(string fileName, string folderPath = "")
        {
            string? documentLoaderUrl = null;
            try
            {
                //var folderPath = ConfigurationManager.AppSettings["UploadedFilesPath"] ?? "";
                var fullPath = UploadFilePath(folderPath, fileName, false);

                if ((!fullPath.StartsWith("~/") && !fullPath.StartsWith("/") && !fullPath.StartsWith("..")) &&
                    File.Exists(fullPath))
                {
                    documentLoaderUrl = $"/handlers/UploadedDocumentLoader.ashx?file={fileName}";
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, "XrmPath caught error on UrlHelper.GetDocumentLoaderLink()");
                Console.WriteLine(ex.Message);
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
                //Serilog.Log.Error(ex, "XrmPath caught error on UrlHelper.UploadFileExists()");
                Console.WriteLine(ex.Message);
            }
            return fileExists;
        }

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
            var currentUrl = HttpHelper.HttpContext?.Request.GetDisplayUrl().ToString();
            if (currentUrl != null && currentUrl.StartsWith("https://"))
            {
                secureUrl = url.Replace("http://", "https://");
            }

            return secureUrl;
        }

        public static string? GetCurrentUrl(string defaultUrl = "Unknown")
        {
            string? currentUrl;
            try
            {
                //currentUrl = HttpContext.Current?.Request.Url.AbsoluteUri ?? defaultUrl;
                currentUrl = HttpHelper.HttpContext?.Request.GetDisplayUrl();
            }
            catch
            {
                currentUrl = defaultUrl;
            }
            return currentUrl;
        }
    }
}
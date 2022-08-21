using System;
using System.Collections.Generic;
using System.Xml.XPath;
using XrmPath.Helpers.Utilities;
using XrmPath.Web.Models;
using umbraco;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using System.Linq;
using XrmPath.Helpers.Model;
using Umbraco.Core;
using Umbraco.Core.Services;
using Newtonsoft.Json.Linq;
using XrmPath.Web.Helpers.Utils;

namespace XrmPath.Web.Helpers.UmbracoHelpers
{
    public static class MediaUtility
    {
        public static MediaItem GetMediaItem(int id)
        {
            try
            {
                //first check media item via UmbracoHelper
                //this pulls from lucene index
                ContextHelper.EnsureUmbracoContext();
                if (UmbracoContext.Current != null)
                {
                    var typeMediaItem = ServiceUtility.UmbracoHelper.TypedMedia(id);
                    //var publishedMediaItem = new MediaValues(typeMediaItem);
                    if (typeMediaItem.NodeExists())
                    {
                        var mediaItem = new MediaItem {
                            Id = typeMediaItem.Id,
                            Url = typeMediaItem.NodeUrl(),
                            Name = typeMediaItem.Name
                        };
                        return mediaItem;
                        //return publishedMediaItem;
                    }
                }

                //lastly if we can't pull it from cache, we'll use the database service umbraco.library.
                //according to https://shazwazza.com/post/ultra-fast-media-performance-in-umbraco/ results are cached so it only makes db call the first time.
                var media = library.GetMedia(id, false);
                if (media?.Current != null)
                {
                    media.MoveNext();
                    var liveMediaItem = new MediaValues(media.Current);
                    if (liveMediaItem.Id > 0)
                    {
                        var mediaItem = new MediaItem {
                            Id = id,
                            Url = SiteUrlUtility.GetSiteUrl(liveMediaItem.Values["umbracoFile"]),
                            Name = liveMediaItem.Name
                        };
                        return mediaItem;
                        //return liveMediaItem;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MediaUtility.GetMediaItem(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return null;
        }
        
        public static MediaItem GetMediaItem(this IPublishedContent content, string alias)
        {
            var mediaList = content.GetMediaList(alias);
            var media = mediaList.FirstOrDefault();
            return media;
        }
        public static MediaItem GetMediaItem(Udi udi)
        {
            var id = ServiceUtility.UmbracoHelper.GetIdForUdi(udi);
            if (id > 0)
            {
                var mediaItem = GetMediaItem(id);
                if (mediaItem != null)
                {
                    return mediaItem;
                }
            }
            return null;
        }

        public static MediaItem GetMediaItem(string udiString)
        {
            Udi udi;
            var validUdi = Udi.TryParse(udiString, out udi);
            if (validUdi)
            { 
                var media = GetMediaItem(udi);
                if (media != null)
                {
                    return media;
                }
            }
            return null;
        }

        public static MediaItem GetMediaItem(this IContent content, string alias)
        {
            var mediaList = content.GetMediaList(alias);
            var media = mediaList.FirstOrDefault();
            return media;
        }

        public static string GetMediaPath(int nodeid)
        {
            var path = string.Empty;
            try
            {
                var mediaItem = GetMediaItem(nodeid);
                if (mediaItem?.Url != null)
                {
                    path = mediaItem.Url;
                    return path;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MediaUtility.GetMediaItem(IContent, alias). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return path;
        }

        /// <summary>
        /// Pulls Media property from either library.GetMedia method or MediaService.GetById
        /// </summary>
        /// <param name="nodeid"></param>
        /// <param name="alias"></param>
        /// <param name="cachedVersion"></param>
        /// <returns></returns>
        public static string GetMediaProperty(int nodeid, string alias = "umbracoFile", bool cachedVersion = true)
        {
            var path = string.Empty;
            try
            {
                if (cachedVersion)
                {
                    //get from library.GetMedia //gets from database the first time, then cache
                    var media = library.GetMedia(nodeid, false);
                    if (media?.Current != null)
                    {
                        media.MoveNext();
                        var xpath = media.Current;
                        var result = xpath?.SelectChildren(XPathNodeType.Element);
                        if (result != null)
                        {
                            while (result.MoveNext())
                            {
                                if (result.Current != null && !result.Current.HasAttributes && result.Current.Name == alias && !string.IsNullOrEmpty(result.Current.Value))
                                {
                                    path = result.Current.Value;
                                    return path;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //get from database
                    var m = ServiceUtility.MediaService.GetById(nodeid);
                    if (m != null && m.Id > 0 && m.HasProperty(alias))
                    {
                        path = m.GetValue(alias) != null ? m.GetValue(alias).ToString() : string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MediaUtility.GetMediaProperty(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node">Content node</param>
        /// <param name="alias">Field alias of media crop picker</param>
        /// <param name="cropAlias">Crop alias</param>
        /// <param name="renderAsExtension">Extension (Ex. png) if different from uploaded</param>
        /// <returns></returns>
        public static string GetMediaCropUrl(this IPublishedContent node, string alias, string cropAlias, string renderAsExtension = "")
        {
            var cropUrl = string.Empty;
            try
            {
                var jsonValue = node.GetContentValue(alias);
                if (!string.IsNullOrEmpty(jsonValue))
                {
                    var publishedContent = ServiceUtility.UmbracoHelper.GetById(node.Id);
                    cropUrl = publishedContent != null ? publishedContent.GetCropUrl(alias, cropAlias) : string.Empty;

                    if (!string.IsNullOrEmpty(renderAsExtension))
                    {
                        var extension = StringUtility.GetExtensionFromRelativeUrlPath(cropUrl);
                        if (cropUrl.IndexOf("format=", StringComparison.Ordinal) == -1 && !string.IsNullOrEmpty(extension) && renderAsExtension != extension)
                        {
                            cropUrl = $"{cropUrl}&format={extension}";
                        }
                    }

                    var brightness = node.GetNodeDecimal(UmbracoCustomFields.CropperBrightness);
                    var contrast = node.GetNodeDecimal(UmbracoCustomFields.CropperContrast);
                    var saturation = node.GetNodeDecimal(UmbracoCustomFields.CropperSaturation);

                    if (brightness != 0)
                    {
                        cropUrl = $"{cropUrl}&brightness={brightness}";
                    }
                    if (contrast != 0)
                    {
                        cropUrl = $"{cropUrl}&contrast={contrast}";
                    }
                    if (saturation != 0)
                    {
                        cropUrl = $"{cropUrl}&saturation={saturation}";
                    }
                }
                cropUrl = SiteUrlUtility.GetSiteUrl(cropUrl);
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MediaUtility.GetMediaCropUrl(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return cropUrl;
        }

        /// <summary>
        /// When Url picker is added as a parameter;
        /// The funciton will return dynamic linked documnet id. 
        /// </summary>
        /// <param name="content">parameter content</param>
        /// <param name="alias">parameter name</param>
        /// <returns></returns>
        //public static int GetDocumentId(this IPublishedContent content, string alias)
        //{
        //    int docId = -1;
        //    try
        //    {
        //        var properties = content.GetContentValue(alias).Replace("[","").Replace("]","");
        //        if (properties.Contains("umb://"))
        //        {
        //            bool foundUdi = false;
        //            dynamic jsonObj = JsonConvert.DeserializeObject(properties);
        //            foreach(Newtonsoft.Json.Linq.JProperty valueObj in jsonObj)
        //            {
        //                if (valueObj.Name  == "udi")
        //                {
        //                    Udi udi;
        //                    var validUdi = Udi.TryParse(valueObj.Value.ToString(), out udi);
        //                    docId = ServiceUtility.UmbracoHelper.GetIdForUdi(udi);
        //                    //UmbracoContext.Current is null
        //                    //var context = UmbracoContext.Current.UrlProvider;
        //                    //url = context.GetUrl(pulishedDoc.Id);
        //                    foundUdi = true;
        //                    break;
        //                }
        //            }
        //            if(!foundUdi)
        //            {
        //                LogHelper.Warn<string>($"XrmPath.Web caught error on on MediaUtility.GetDocumentUrl(): Type:{typeof(T)} Url({UrlUtility.GetCurrentUrl()}) Error: {properties.ToString()}");
        //            }
        //        }
        //        else
        //        {
        //            LogHelper.Warn<string>($"XrmPath.Web caught error on on MediaUtility.GetDocumentUrl(): Type:{typeof(T)} Url({UrlUtility.GetCurrentUrl()}) Error: {"umb:// could not be found."}");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Error<string>($"XrmPath.Web caught error on MediaUtility.GetDocumentUrl(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
        //    }
        //    return docId;
        //}

        public static List<MediaItem> GetMediaList(this IPublishedContent content, string alias)
        {
            var mediaList = new List<MediaItem>();
            try
            {
                var files = content.GetContentValue(alias);
                //mediaList = GetMediaList(files);
                if (files.Contains("umb://"))
                {
                    //Umbraco.MediaPicker2
                    var udisList = files.Split(',').ToList();
                    foreach (var udiValue in udisList)
                    {
                        Udi udi;
                        var validUdi = Udi.TryParse(udiValue, out udi);
                        if (validUdi && udi != null)
                        { 
                            var mediaItem = GetMediaItem(udi);
                            if (mediaItem != null)
                            {
                                mediaList.Add(mediaItem);
                            }
                        }
                    }
                }
                else
                { 
                    //Umbraco.MediaPicker
                    var mediaIds = files.Split(',');

                    foreach (var mediaIdValue in mediaIds)
                    {
                        int mediaId;
                        bool validInt = int.TryParse(mediaIdValue, out mediaId);
                        if (validInt && mediaId > 0)
                        { 
                            var filePath = GetMediaItem(mediaId)?.Url ?? string.Empty;
                            var relatedFile = new MediaItem
                            {
                                Id = mediaId,
                                Url = filePath
                            };
                            if (relatedFile != null)
                            {
                                mediaList.Add(relatedFile);
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MediaUtility.GetMediaList(IPublishedContent). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return mediaList;
        }

        public static List<MediaItem> GetMediaList(this IContent content, string alias)
        {
            var mediaList = new List<MediaItem>();
            try
            {
                var files = content.GetContentValue(alias);
                //mediaList = GetMediaList(files);
                if (files.Contains("umb://"))
                {
                    //Umbraco.MediaPicker2
                    var udisList = files.Split(',').Select(Udi.Parse).ToList();
                    var multiMediaPicker = ServiceUtility.ContentService.GetByIds(udisList).ToList();

                    if (multiMediaPicker.Any())
                    {
                        var multiMediaList = multiMediaPicker.Select(i => new MediaItem {
                            Id = i.Id,
                            Name = i.Name,
                            Url = i.GetUrl()
                        });
                        mediaList.AddRange(multiMediaList);
                    }
                }
                else
                { 
                    //Umbraco.MediaPicker
                    var mediaIds = files.Split(',');
                    foreach (var mediaIdValue in mediaIds)
                    {
                        int mediaId;
                        bool validInt = int.TryParse(mediaIdValue, out mediaId);
                        if (validInt && mediaId > 0)
                        { 
                            var filePath = GetMediaItem(mediaId)?.Url ?? string.Empty;
                            var relatedFile = new MediaItem
                            {
                                Id = mediaId,
                                Url = filePath
                            };
                            if (relatedFile != null)
                            {
                                mediaList.Add(relatedFile);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on MediaUtility.GetMediaList(IContent). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return mediaList;
        }

        public static List<GenericModel> PublishAllMediaNodes(int mediaId)
        {
            var content = ServiceUtility.MediaService.GetById(mediaId);
            var descendants = content.Descendants();

            var genericList = new List<GenericModel>();

            ServiceUtility.MediaService.Save(content);
            var rootItem = new GenericModel { Text = content.Name, Id = content.Id };
            genericList.Add(rootItem);

            foreach(var descendant in descendants)
            {
                ServiceUtility.MediaService.Save(descendant);
                var item = new GenericModel { Text = descendant.Name, Id = descendant.Id };
                genericList.Add(item);
            }
            return genericList;
        } 
    }
}
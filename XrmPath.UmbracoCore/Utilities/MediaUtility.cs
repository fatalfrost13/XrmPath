using System;
using System.Collections.Generic;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Models;
using Umbraco.Core.Models;
using Umbraco.Web;
using System.Linq;
using XrmPath.Helpers.Model;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models.PublishedContent;
using XrmPath.UmbracoCore.Helpers;

namespace XrmPath.UmbracoCore.Utilities
{
    public static class MediaUtility
    {
        public static MediaItem GetMediaItem(int id)
        {
            try
            {
                //first check media item via UmbracoHelper
                //this pulls from lucene index
                
                //var umbracoHelper = Umbraco.Web.Composing.Current.UmbracoHelper;
                var typeMediaItem = ServiceUtility.UmbracoHelper.Media(id);
                //var publishedMediaItem = new MediaValues(typeMediaItem);
                if (typeMediaItem != null && typeMediaItem.NodeExists())
                {
                    var mediaItem = new MediaItem
                    {
                        Id = typeMediaItem.Id,
                        Url = typeMediaItem.NodeUrl(),
                        Name = typeMediaItem.Name
                    };
                    return mediaItem;
                    //return publishedMediaItem;
                }
                else
                {
                    //lastly if we can't pull it from cache, we'll use the database service umbraco.library.
                    //according to https://shazwazza.com/post/ultra-fast-media-performance-in-umbraco/ results are cached so it only makes db call the first time.
                    var media = ServiceUtility.MediaService.GetById(id);
                    if (media != null)
                    {
                        var mediaItem = new MediaItem
                        {
                            Id = id,
                            Url = media.GetValue("umbracoFile").ToString(), //  SiteUrlUtility.GetSiteUrl(liveMediaItem.Values["umbracoFile"]),
                            Name = media.Name
                        };
                        return mediaItem;
                        //return liveMediaItem;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaItem(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on MediaUtility.GetMediaItem(). URL Info: {UrlUtility.GetCurrentUrl()}");
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
            //var id = ServiceUtility.UmbracoHelper.GetIdForUdi(udi);
            var id = ServiceUtility.UmbracoHelper.Media(udi)?.Id ?? 0;
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
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on MediaUtility.GetMediaItem(IContent, alias). URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaItem(IContent, alias). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
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
                
                //get from database
                var m = ServiceUtility.MediaService.GetById(nodeid);
                if (m != null && m.Id > 0 && m.HasProperty(alias))
                {
                    path = m.GetValue(alias) != null ? m.GetValue(alias).ToString() : string.Empty;
                }
                
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on MediaUtility.GetMediaProperty(). URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaProperty(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
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
                cropUrl = SiteUrlHelper.GetSiteUrl(cropUrl);
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaCropUrl(). URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaCropUrl(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return cropUrl;
        }

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
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaList(IPublishedContent). URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaList(IPublishedContent). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
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
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaList(IContent). URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaList(IContent). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return mediaList;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using XrmPath.Helpers.Utilities;
using XrmPath.Web.Models;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace XrmPath.Web.Helpers.UmbracoHelpers
{
    //This Utility will make database calls and does not use Umbraco Cached content

    public static class ContentUtility
    {
        
        public static IEnumerable<IContentType> GetContentTypes()
        {
            var contentTypes = ServiceUtility.ContentTypeService.GetAllContentTypes();
            return contentTypes;
        }

        public static bool NodeExists(this IContent content)
        {
            return content != null && content.Id > 0;
        }

        public static string GetContentValue(this IContent content, string propertyAlias, string defaultValue = "")
        {
            var result = defaultValue;
            try
            {
                if (content.NodeExists() && content.HasProperty(propertyAlias))
                {
                    var property = content.GetValue(propertyAlias);
                    if (!string.IsNullOrEmpty(property?.ToString()))
                    {
                        result = property.ToString();
                    }
                    //result = TemplateUtilities.ParseInternalLinks(result);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on ContentUtility.GetContentValue() for DocumentTypeAlias: {propertyAlias}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return result;
        }

        public static string GetContentValue(this IContent content, ISet<string> propertyAliases, string defaultValue = "")
        {
            var result = defaultValue;
            try
            {
                if (content.NodeExists())
                {
                    foreach (var propertyAlias in propertyAliases)
                    {
                        var hasProperty = content.HasProperty(propertyAlias);
                        if (hasProperty)
                        {
                            var property = content.GetValue(propertyAlias);
                            if (!string.IsNullOrEmpty(property?.ToString()))
                            {
                                result = property.ToString();
                            }

                            //result = TemplateUtilities.ParseInternalLinks(result);
                        }

                        if (!string.IsNullOrEmpty(result))
                        {
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var aliases = string.Join(",", propertyAliases);
                LogHelper.Error<string>($"XrmPath.Web caught error on ContentUtility.GetContentValue() for DocumentTypeAliases: {aliases}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return result;
        }

        public static IPublishedContent ToPublishedContent(this IContent content)
        {
            if (content.NodeExists() && content.Published)
            {
                var publishedContent = ServiceUtility.UmbracoHelper.GetById(content.Id);
                if (publishedContent.NodeExists())
                {
                    return publishedContent;
                }
            }
            return null;
        }

        public static string GetTitle(this IContent content, string aliases = "title,pageTitle,name")
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
                        var title = content.GetContentValue(alias);
                        if (!string.IsNullOrEmpty(title))
                        {
                            return title;
                        }
                    }
                }
                else
                {
                    strTitle = content.GetContentValue(aliases);
                }
            }
            return strTitle;
        }

        public static string GetDescription(this IContent content, string aliases = "")
        {
            var desc = string.Empty;
            if (string.IsNullOrEmpty(aliases))
            {
                aliases = $"{UmbracoCustomFields.Description},{UmbracoCustomFields.MetaDescription}";
            }
            if (aliases.Contains(","))
            {
                var aliasList = aliases.StringToSet();
                foreach (var alias in aliasList)
                {
                    desc = content.GetContentValue(alias);
                    if (!string.IsNullOrEmpty(desc))
                    {
                        return desc;
                    }
                }
            }
            else
            {
                desc = content.GetContentValue(aliases);
            }
            return desc;
        }

        public static int FindChildNodeId(this IContent content, ISet<string> nodeTypeAliasSet)
        {
            var firstChildNode = content.FindChildNode(nodeTypeAliasSet)?.Id ?? 0;
            return firstChildNode;
        }
        public static int FindChildNodeId(this IContent content, string nodeTypeAlias)
        {
            var firstChildNode = content.FindChildNode(nodeTypeAlias)?.Id ?? 0;
            return firstChildNode;
        }
        public static IContent FindChildNode(this IContent content, ISet<string> nodeTypeAliasSet)
        {
            if (content == null || content.Id == 0 || content.Children() == null) return null;
            return content.Children().FirstOrDefault(child => child.NodeExists() && nodeTypeAliasSet.Contains(child.ContentType.Alias));
        }
        public static IContent FindChildNode(this IContent content, string nodeTypeAlias)
        {
            if (content == null || content.Id == 0 || content.Children() == null) return null;
            return content.Children().FirstOrDefault(child => child.NodeExists() && string.Equals(nodeTypeAlias, child.ContentType.Alias, StringComparison.Ordinal));
        }
        private static IEnumerable<IContent> FindAllNodesByAlias(this IContent content, ISet<string> nodeTypeAliasSet = null)
        {
            if (content == null || content.Id == 0) yield break;
            if (nodeTypeAliasSet == null || nodeTypeAliasSet.Contains(content.ContentType.Alias)) yield return content;
            foreach (var child in content.Children().SelectMany(child => FindAllNodesByAlias(child, nodeTypeAliasSet)))
                yield return child;
        }
        private static IEnumerable<IContent> FindAllNodesByAlias(this IContent content, string nodeTypeAlias = "")
        {
            if (content == null || content.Id == 0) yield break;
            if (string.Equals(nodeTypeAlias, content.ContentType.Alias, StringComparison.Ordinal) || string.IsNullOrEmpty(nodeTypeAlias)) yield return content;
            foreach (var child in content.Children().SelectMany(child => FindAllNodesByAlias(child, nodeTypeAlias)))
                yield return child;
        }

        public static IEnumerable<IContent> FindAllNodes(this IContent content, string nodeTypeAlias)
        {
            var contentNodes = content.FindAllNodesByAlias("").Where(i => i.ContentType.Alias.Equals(nodeTypeAlias));
            return contentNodes;
        }
        public static IEnumerable<IContent> FindAllNodes(this IContent content, ISet<string> nodeTypeAliasSet)
        {
            var contentNodes = content.FindAllNodesByAlias("").Where(i => nodeTypeAliasSet.Contains(content.ContentType.Alias));
            return contentNodes;
        }

        public static string GetDate(this IContent content, string dateFormat = "", string alias = "date")
        {
            var date = GetDateTime(content, alias);
            if (string.IsNullOrEmpty(dateFormat))
            {
                dateFormat = ConfigurationManager.AppSettings["dateFormat"];
            }
            var strDate = date.ToString(dateFormat);
            return strDate;
        }

        public static DateTime GetDateTime(this IContent content, string alias = "date")
        {
            var date = content.CreateDate;
            var dateValue = content.GetContentValue(alias);
            if (!string.IsNullOrEmpty(dateValue))
            {
                date = Convert.ToDateTime(dateValue);
            }
            return date;
        }

        public static DateTime? GetNullableDateTime(this IContent content, string alias = "date", DateTime? defaultDate = null)
        {
            var date = defaultDate;
            var dateValue = content.GetContentValue(alias);
            if (!string.IsNullOrEmpty(dateValue))
            {
                DateTime parseDate;
                var validDate = DateTime.TryParse(dateValue, out parseDate);
                if (validDate && parseDate > DateTime.MinValue)
                {
                    date = Convert.ToDateTime(dateValue);
                }
            }
            return date;
        }
        public static string GetUrl(this IContent content, string alias = "urlPicker")
        {
            if (content.NodeExists() && content.Published)
            {
                var publishedContent = content.ToPublishedContent();
                if (publishedContent != null)
                {
                    return publishedContent.GetUrl();
                }
            }
            return string.Empty;
        }
        public static int GetContentPickerId(this IContent content, string alias)
        {
            var intValue = 0;
            var nodeList = content.GetNodeList(alias);
            if (nodeList.Any())
            {
                intValue = nodeList.First().Id;
            }
            return intValue;
        }
        public static int GetNodeInt(this IContent content, string alias)
        {
            var intValue = 0;
            var nodeValue = content.GetContentValue(alias);
            if (!string.IsNullOrEmpty(nodeValue))
            {
                int.TryParse(nodeValue, out intValue);
            }

            return intValue;
        }
        public static IContent GetContentPicker(this IContent content, string alias)
        {
            IContent node = null;
            var nodeList = content.GetNodeList(alias);
            if (nodeList.Any())
            {
                node = nodeList.First();
            }
            return node;
        }
        public static decimal GetNodeDecimal(this IContent content, string alias, decimal defaultValue = 0)
        {
            var decValue = defaultValue;
            var contentValue = content.GetContentValue(alias);

            if (!string.IsNullOrEmpty(contentValue))
            {
                decimal.TryParse(contentValue, out decValue);
            }
            return decValue;
        }
        public static double GetNodeDouble(this IContent content, string alias, double defaultValue = 0)
        {
            var dbValue = defaultValue;
            var contentValue = content.GetContentValue(alias);

            if (!string.IsNullOrEmpty(contentValue))
            {
                double.TryParse(contentValue, out dbValue);
            }
            return dbValue;
        }
        public static bool GetNodeBoolean(this IContent content, string alias)
        {
            var contentValue = content.GetContentValue(alias);
            var boolValue = StringUtility.ToBoolean(contentValue);
            return boolValue;
        }
       
        public static List<IContent> GetNodeList(this IContent content, string alias)
        {
            var contentValue = content.GetContentValue(alias);
            try
            {
                if (!string.IsNullOrEmpty(contentValue))
                {
                    if (!contentValue.Contains("umb://"))
                    {
                        //Umbraco.MultiNodeTreePicker
                        var nodeList = new List<IContent>();
                        var idsList = contentValue.Split(',').ToList();
                        foreach(var idValue in idsList)
                        {
                            int id;
                            var validId = int.TryParse(idValue, out id);
                            if (validId && id > 0)
                            {
                                var node = ServiceUtility.ContentService.GetById(id);
                                if (node.NodeExists())
                                {
                                    nodeList.Add(node);
                                }
                            }
                        }
                        return nodeList;
                    }
                    else
                    {
                        //Umbraco.MultiNodeTreePicker2
                        List<IContent> nodeList = new List<IContent>();
                        var udisList = contentValue.Split(',').ToList();
                        foreach (var udiValue in udisList)
                        {
                            Udi udi;
                            var validUdi = Udi.TryParse(udiValue, out udi);
                            if (validUdi && udi != null)
                            { 
                                var id = ServiceUtility.UmbracoHelper.GetIdForUdi(udi);
                                var contentPicker = ServiceUtility.ContentService.GetById(id);
                                if (contentPicker.NodeExists())
                                {
                                    nodeList.Add(contentPicker);
                                }
                            }
                        }
                        return nodeList;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on ContentUtility.GetNodeList(). Content Value: {contentValue}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return new List<IContent>();
        }

        public static IEnumerable<IContent> GetNodeListUnpublished(this ISet<int> intList)
        {
            if (intList.Any())
            {
                return intList.Select(i => ServiceUtility.ContentService.GetById(i)).Where(i => i.NodeExists());
            }
            return Enumerable.Empty<IContent>();
        }
        public static string GetContentColor(this IContent content, string alias, string defaultColor = null)
        {
            var color = !string.IsNullOrEmpty(content.GetContentValue(alias)) ? content.GetContentValue(alias) : null;
            if (color != null && !color.StartsWith("#"))
            {
                color = $"#{color}";
            }
            if (color == null && !string.IsNullOrEmpty(defaultColor))
            {
                color = defaultColor;
            }

            return color;
        }
        public static List<string> GetTagList(this IContent content, string fieldAlias)
        {
            var tagList = new List<string>();
            var tagItems = content?.GetContentValue(fieldAlias);

            if (!string.IsNullOrEmpty(tagItems) && tagItems.StartsWith("[") && tagItems.EndsWith("]"))
            {
                //using new json tag format
                tagList = JsonConvert.DeserializeObject<List<string>>(tagItems);
            }
            else if (!string.IsNullOrEmpty(tagItems))
            {
                //using old csv tag format
                tagList = tagItems.Split(',').Select(i => i.Trim()).Where(i => !string.IsNullOrEmpty(i)).ToList();
            }
            return tagList;
        }

        public static string GetTarget(this IContent content, string alias = "urlPicker")
        {
            string strTarget = "_self";
            if (content.NodeExists())
            {
                //check URL Property
                var urlPickerValue = content.GetContentValue(alias);
                if (!string.IsNullOrWhiteSpace(urlPickerValue))
                {
                    var urlPicker = MultiUrlUtility.GetUrlPicker(content, alias);
                    strTarget = urlPicker.NewWindow ? "_blank" : "_self";
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
        public static bool HasPendingChanges(this IContent content)
        {
            if (!content.Published || !content.Version.Equals(content.PublishedVersionGuid))
            {
                return true;
            }
            return false;
        }
        
      public static string GetUdiString (this IContent content)
        {
            var udi = string.Empty;

            try
            {
                udi = Udi.Create(Constants.UdiEntityType.Document, content.Key).ToString();
                //var udi = Udi.Create(Constants.UdiEntityType.Document, content.GetKey()).ToString();
            }
            catch(Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on ContentUtility.GetUdiString(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            
            return udi;
        }
    }
}
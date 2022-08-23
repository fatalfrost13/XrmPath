using XrmPath.Helpers.Utilities;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core;
using XrmPath.UmbracoCore.Definitions;

namespace XrmPath.UmbracoCore.Utilities
{
    //This Utility will make database calls and does not use Umbraco Cached content

    public class ContentUtility: BaseInitializer
    {
        private readonly UmbracoHelper? _umbracoHelper;
        private readonly IContentService? _contentService;
        protected readonly IContentTypeService? _contentTypeService;
        public ContentUtility(ServiceUtility? serviceUtil): base(serviceUtil)
        {
            if (_umbracoHelper == null)
            {
                _umbracoHelper = _serviceUtil?.GetUmbracoHelper();
            }
            if (_contentService == null)
            {
                _contentService = _serviceUtil?.GetContentService();
            }
            if (_contentTypeService == null)
            {
                _contentTypeService = _serviceUtil?.GetContentServiceType();
            }
        }

        public IEnumerable<IContentType> GetContentTypes()
        {
            var contentTypes = _contentTypeService?.GetAll();
            return contentTypes ?? Enumerable.Empty<IContentType>();
        }

        public bool NodeExists(IContent? content)
        {
            return content != null && content.Id > 0;
        }

        public string GetContentValue(IContent? content, string propertyAlias, string defaultValue = "")
        {
            var result = defaultValue;
            try
            {
                if (NodeExists(content) && (content?.HasProperty(propertyAlias) ?? false))
                {
                    var property = content?.GetValue(propertyAlias);
                    if (!string.IsNullOrEmpty(property?.ToString()))
                    {
                        result = property.ToString();
                    }
                    //result = TemplateUtilities.ParseInternalLinks(result);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on ContentUtility.GetContentValue() for DocumentTypeAlias: {propertyAlias}. URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on ContentUtility.GetContentValue() for DocumentTypeAlias: {propertyAlias}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return result ?? "";
        }

        public string GetContentValue(IContent content, ISet<string> propertyAliases, string defaultValue = "")
        {
            var result = defaultValue;
            try
            {
                if (NodeExists(content))
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
            catch(Exception ex)
            {
                //var aliases = string.Join(",", propertyAliases);
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on ContentUtility.GetContentValue() for DocumentTypeAliases: {aliases}. URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on ContentUtility.GetContentValue() for DocumentTypeAliases: {aliases}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return result ?? "";
        }

        public IPublishedContent? ToPublishedContent(IContent content)
        {
            if (NodeExists(content) && content.Published)
            {
                var publishedContent = _umbracoHelper?.Content(content.Id);
                if (pcUtil?.NodeExists(publishedContent) ?? false)
                {
                    return publishedContent;
                }
            }
            return null;
        }

        public string GetTitle(IContent content, string aliases = "title,pageTitle,name")
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
                        var title = GetContentValue(content,alias);
                        if (!string.IsNullOrEmpty(title))
                        {
                            return title;
                        }
                    }
                }
                else
                {
                    strTitle = GetContentValue(content, aliases);
                }
            }
            return strTitle ?? "";
        }

        public string GetDescription(IContent content, string aliases = "")
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
                    desc = GetContentValue(content, alias);
                    if (!string.IsNullOrEmpty(desc))
                    {
                        return desc;
                    }
                }
            }
            else
            {
                desc = GetContentValue(content, aliases);
            }
            return desc;
        }

        public IEnumerable<IContent> GetChildren(IContent content) {
            var children = _contentService?.GetPagedChildren(content.Id, 1, 1000, out long totalRecords);
            return children ?? Enumerable.Empty<IContent>();
        }

        public int FindChildNodeId(IContent content, ISet<string> nodeTypeAliasSet)
        {
            var firstChildNode = FindChildNode(content, nodeTypeAliasSet)?.Id ?? 0;
            return firstChildNode;
        }
        public int FindChildNodeId(IContent content, string nodeTypeAlias)
        {
            var firstChildNode = FindChildNode(content, nodeTypeAlias)?.Id ?? 0;
            return firstChildNode;
        }
        public IContent? FindChildNode(IContent content, ISet<string> nodeTypeAliasSet)
        {
            if (content == null || content.Id == 0 || GetChildren(content) == null) return null;
            return GetChildren(content).FirstOrDefault(child => NodeExists(child) && nodeTypeAliasSet.Contains(child.ContentType.Alias));
        }
        public IContent? FindChildNode(IContent content, string nodeTypeAlias)
        {
            if (content == null || content.Id == 0 || GetChildren(content) == null) return null;
            return GetChildren(content).FirstOrDefault(child => NodeExists(child) && string.Equals(nodeTypeAlias, child.ContentType.Alias, StringComparison.Ordinal));
        }
        private IEnumerable<IContent> FindAllNodesByAlias(IContent content, ISet<string>? nodeTypeAliasSet = null)
        {
            if (content == null || content.Id == 0) yield break;
            if (nodeTypeAliasSet == null || nodeTypeAliasSet.Contains(content.ContentType.Alias)) yield return content;
            foreach (var child in GetChildren(content).SelectMany(child => FindAllNodesByAlias(child, nodeTypeAliasSet)))
                yield return child;
        }
        private IEnumerable<IContent> FindAllNodesByAlias(IContent content, string nodeTypeAlias = "")
        {
            if (content == null || content.Id == 0) yield break;
            if (string.Equals(nodeTypeAlias, content.ContentType.Alias, StringComparison.Ordinal) || string.IsNullOrEmpty(nodeTypeAlias)) yield return content;
            foreach (var child in GetChildren(content).SelectMany(child => FindAllNodesByAlias(child, nodeTypeAlias)))
                yield return child;
        }

        public IEnumerable<IContent> FindAllNodes(IContent content, string nodeTypeAlias)
        {
            var contentNodes = FindAllNodesByAlias(content, "").Where(i => i.ContentType.Alias.Equals(nodeTypeAlias));
            return contentNodes;
        }
        public IEnumerable<IContent> FindAllNodes(IContent content, ISet<string> nodeTypeAliasSet)
        {
            var contentNodes = FindAllNodesByAlias(content,"").Where(i => nodeTypeAliasSet.Contains(content.ContentType.Alias));
            return contentNodes;
        }

        public string GetDate(IContent content, string dateFormat = "", string alias = "date")
        {
            var date = GetDateTime(content, alias);
            //if (string.IsNullOrEmpty(dateFormat))
            //{
            //    dateFormat = ConfigurationManager.AppSettings["dateFormat"];
            //}
            var strDate = date.ToString(dateFormat);
            return strDate;
        }

        public DateTime GetDateTime(IContent content, string alias = "date")
        {
            var date = content.CreateDate;
            var dateValue = GetContentValue(content, alias);
            if (!string.IsNullOrEmpty(dateValue))
            {
                date = Convert.ToDateTime(dateValue);
            }
            return date;
        }

        public DateTime? GetNullableDateTime(IContent content, string alias = "date", DateTime? defaultDate = null)
        {
            var date = defaultDate;
            var dateValue = GetContentValue(content, alias);
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
        public string GetUrl(IContent content, string alias = "urlPicker")
        {
            if (NodeExists(content) && content.Published)
            {
                var publishedContent = ToPublishedContent(content);
                if (publishedContent != null)
                {
                    return publishedContent.Url();
                }
            }
            return string.Empty;
        }
        public int GetContentPickerId(IContent content, string alias)
        {
            var intValue = 0;
            var nodeList = GetNodeList(content, alias);
            if (nodeList.Any())
            {
                intValue = nodeList.First().Id;
            }
            return intValue;
        }
        public int GetNodeInt(IContent content, string alias)
        {
            var intValue = 0;
            var nodeValue = GetContentValue(content, alias);
            if (!string.IsNullOrEmpty(nodeValue))
            {
                int.TryParse(nodeValue, out intValue);
            }

            return intValue;
        }
        public IContent? GetContentPicker(IContent content, string alias)
        {
            IContent? node = null;
            var nodeList = GetNodeList(content, alias);
            if (nodeList.Any())
            {
                node = nodeList.First();
            }
            return node;
        }
        public decimal GetNodeDecimal(IContent content, string alias, decimal defaultValue = 0)
        {
            var decValue = defaultValue;
            var contentValue = GetContentValue(content, alias);

            if (!string.IsNullOrEmpty(contentValue))
            {
                decimal.TryParse(contentValue, out decValue);
            }
            return decValue;
        }
        public double GetNodeDouble(IContent content, string alias, double defaultValue = 0)
        {
            var dbValue = defaultValue;
            var contentValue = GetContentValue(content, alias);

            if (!string.IsNullOrEmpty(contentValue))
            {
                double.TryParse(contentValue, out dbValue);
            }
            return dbValue;
        }
        public bool GetNodeBoolean(IContent content, string alias)
        {
            var contentValue = GetContentValue(content, alias);
            var boolValue = StringUtility.ToBoolean(contentValue);
            return boolValue;
        }
       
        public List<IContent> GetNodeList(IContent content, string alias)
        {
            var contentValue = GetContentValue(content, alias);
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
                                var node = _contentService?.GetById(id);
                                if (NodeExists(node) && node != null)
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
                            Udi udi = Udi.Create(udiValue);
                            if (udi != null)
                            { 
                                var id = _umbracoHelper?.Content(udi)?.Id ?? 0;
                                if (id > 0)
                                {
                                    var contentPicker = _contentService?.GetById(id);
                                    if (NodeExists(contentPicker) && contentPicker != null)
                                    {
                                        nodeList.Add(contentPicker);
                                    }
                                }
                            }
                        }
                        return nodeList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on ContentUtility.GetNodeList(). Content Value: {contentValue}. URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on ContentUtility.GetNodeList(). Content Value: {contentValue}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return new List<IContent>();
        }

        public IEnumerable<IContent?> GetNodeListUnpublished(ISet<int> intList)
        {
            if (intList.Any())
            {
                return intList.Select(i => _contentService?.GetById(i)).Where(i => NodeExists(i));
            }
            return Enumerable.Empty<IContent>();
        }
        public string GetContentColor(IContent content, string alias, string? defaultColor = null)
        {
            var color = !string.IsNullOrEmpty(GetContentValue(content, alias)) ? GetContentValue(content, alias) : null;
            if (color != null && !color.StartsWith("#"))
            {
                color = $"#{color}";
            }
            if (color == null && !string.IsNullOrEmpty(defaultColor))
            {
                color = defaultColor;
            }

            return color ?? "";
        }
        public List<string> GetTagList(IContent content, string fieldAlias)
        {
            var tagList = new List<string>();
            var tagItems = GetContentValue(content, fieldAlias);

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
            return tagList ?? new List<string>();
        }

        public string GetTarget(IContent content, string alias = "urlPicker")
        {
            string strTarget = "_self";
            if (NodeExists(content))
            {
                //check URL Property
                var urlPickerValue = GetContentValue(content, alias);
                if (!string.IsNullOrWhiteSpace(urlPickerValue))
                {
                    
                    var urlPicker = urlUtil?.GetUrlPicker(content, alias);
                    strTarget = (urlPicker?.NewWindow ?? false) ? "_blank" : "_self";
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
        public bool HasPendingChanges(IContent content)
        {
            if (!content.Published || !HasPendingChanges(content))
            {
                return true;
            }
            return false;
        }
        
      public string GetUdiString (IContent content)
        {
            var udi = string.Empty;

            try
            {
                udi = Udi.Create(Constants.UdiEntityType.Document, content.Key).ToString();
                //var udi = Udi.Create(Constants.UdiEntityType.Document, content.GetKey()).ToString();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on ContentUtility.GetUdiString(). URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on ContentUtility.GetUdiString(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            
            return udi;
        }
    }
}
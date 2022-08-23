using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.Utilities
{
    public class PublishedContentUtility: BaseUtility
    {

        protected MultiUrlUtility _urlUtil;
        protected ContentUtility? _contentUtil;
        protected LoggingUtility? _loggingUtil;
        public PublishedContentUtility(ILogger<object>? iLogger = null, UmbracoHelper? umbracoHelper = null, IMediaService? mediaService = null, IExamineManager? examineManager = null, IContentService? contentService = null, IContentTypeService? contentTypeService = null, IOptions<AppSettingsModel>? appSettings = null) 
            : base(iLogger, umbracoHelper, mediaService, examineManager, contentService, contentTypeService, appSettings)
        {
            _urlUtil = new MultiUrlUtility(this);
        }
        public LoggingUtility? GetLoggingUtility()
        {
            if (_loggingUtil == null && _iLogger != null)
            {
                _loggingUtil = new LoggingUtility(_iLogger);
            }
            return _loggingUtil;
        }
        public UmbracoHelper? GetUmbracoHelper() {
            return _umbracoHelper;
        }
        public IMediaService? GetMediaService()
        {
            return _mediaService;
        }
        public IExamineManager? GetExamineManager()
        {
            return _examineManager;
        }
        public IContentService? GetContentService()
        {
            return _contentService;
        }
        public IContentTypeService? GetContentServiceType()
        {
            return _contentTypeService;
        }
        public MultiUrlUtility? GetMultiUrlUtility()
        {
            return _urlUtil;
        }
        
        public ContentUtility? GetContentUtility()
        {
            if (_contentUtil == null) {
                _contentUtil = new ContentUtility(this);
            }
            return _contentUtil;
        }


        public bool NodeExists(IPublishedContent? content)
        {
            if (content != null && content.Id > 0)
            {
                return true;
            }
            return false;
        }

        public string GetContentValue(IPublishedContent? content, string propertyAlias, string defaultValue = "")
        {
            var result = defaultValue;
            if (string.IsNullOrEmpty(propertyAlias))
            {
                return result;
            }
            try
            {
                if (NodeExists(content))
                {
                    var property = content?.GetProperty(propertyAlias);
                    if (property != null && property.HasValue() && !string.IsNullOrEmpty(property.GetValue()?.ToString()))
                    {
                        result = property.GetValue()?.ToString();
                    }

                    //result = TemplateUtilities.ParseInternalLinks(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentValue() for DocumentTypeAlias: {propertyAlias}. URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentValue() for DocumentTypeAlias: {propertyAlias}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return result ?? String.Empty;
        }

        public string GetUrl(IPublishedContent? content, string alias = "urlPicker")
        {
            var strUrl = "";
            if (NodeExists(content))
            {
                //var reverseProxyFolder = SiteUrlUtility.GetRootFromReverseProxy();
                var nodeUrl = content?.Url();
                strUrl = nodeUrl;

                if (!string.IsNullOrEmpty(alias))
                {
                    //check URL Property
                    var stringData = GetContentValue(content, alias);
                    if (!string.IsNullOrWhiteSpace(stringData))
                    {
                        strUrl = _urlUtil.UrlPickerLink(content, alias, "Url");
                        if (string.IsNullOrWhiteSpace(strUrl))
                        {
                            strUrl = nodeUrl;
                        }
                    }
                }
            }
            return strUrl ?? "";
        }

        public string GetTarget(IPublishedContent? content, string alias = "urlPicker")
        {
            string strTarget = "_self";
            if (NodeExists(content))
            {
                //check URL Property
                var stringData = GetContentValue(content, alias);
                if (!string.IsNullOrWhiteSpace(stringData))
                {
                    strTarget = _urlUtil.UrlPickerLink(content, alias, "Target");
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
        public string GetTitle(IPublishedContent? content, string aliases = "title,navigationTitle,name")
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
                        var title = GetContentValue(content, alias);
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

        public decimal GetNodeDecimal(IPublishedContent? content, string alias, decimal defaultValue = 0)
        {
            var decValue = defaultValue;
            var contentValue = GetContentValue(content, alias);

            if (!string.IsNullOrEmpty(contentValue))
            {
                decimal.TryParse(contentValue, out decValue);
            }
            return decValue;
        }

        public double GetNodeDouble(IPublishedContent content, string alias, double defaultValue = 0)
        {
            var dbValue = defaultValue;
            var contentValue = GetContentValue(content, alias);

            if (!string.IsNullOrEmpty(contentValue))
            {
                double.TryParse(contentValue, out dbValue);
            }
            return dbValue;
        }

        public int GetNodeInt(IPublishedContent content, string alias)
        {
            var intValue = 0;
            var nodeValue = GetContentValue(content, alias);
            if (!string.IsNullOrEmpty(nodeValue))
            {
                int.TryParse(nodeValue, out intValue);
            }

            return intValue;
        }
        public bool GetNodeBoolean(IPublishedContent content, string alias, bool? defaultBoolean = null)
        {
            bool boolValue;
            var contentValue = GetContentValue(content, alias);

            if (defaultBoolean != null && string.IsNullOrEmpty(contentValue))
            {
                boolValue = (bool)defaultBoolean;
            }
            else
            {
                boolValue = StringUtility.ToBoolean(contentValue);
            }
            return boolValue;
        }
        public List<IPublishedContent> GetNodeList(IPublishedContent? content, string alias)
        {
            var nodeList = new List<IPublishedContent>();
            var contentValue = GetContentValue(content, alias);

            try
            {
                if (!string.IsNullOrEmpty(contentValue))
                {

                    //var obj = content?.Value(alias); //Model.Value<IEnumerable<IPublishedContent>>("featuredArticles");
                    var multiNode = content?.Value<IEnumerable<IPublishedContent>>(alias);
                    IPublishedContent? singleNode = null;

                    if (multiNode == null)
                    {
                        singleNode = content?.Value<IPublishedContent>(alias) ?? null;
                        if (singleNode != null) {
                            nodeList.Add(singleNode);
                        }
                    }
                    else
                    {
                        nodeList = multiNode.ToList();
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetNodeList(). Content Value: {contentValue}. URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetNodeList(). Content Value: {contentValue}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return nodeList;
        }

        public IEnumerable<IPublishedContent?> GetNodeList(ISet<int> intList)
        {
            if (intList.Any() && _umbracoHelper != null)
            {
                var nodeList = intList.Select(i => _umbracoHelper.Content(i)).Where(i => NodeExists(i));
                return nodeList;
            }
            return Enumerable.Empty<IPublishedContent>();
        }
        public IEnumerable<int> GetNodeIdsInherit(IPublishedContent? content, string alias)
        {
            var nodeListIds = GetNodesInherit(content).SelectMany(i => GetNodeIdsSingle(i, alias));
            var returnIds = nodeListIds.Distinct();
            return returnIds;
        }

        public IEnumerable<int> GetNodeIdsSingle(IPublishedContent? content, string alias)
        {
            var nodeList = GetNodeList(content, alias);
            var nodeIds = nodeList.Select(i => i.Id);
            return nodeIds;
        }
        public IEnumerable<IPublishedContent> GetNodesInherit(IPublishedContent? content, string alias = "")
        {
            return (content?.AncestorsOrSelf().Where(i => string.IsNullOrEmpty(alias) || string.Equals(alias, i.ContentType.Alias, StringComparison.Ordinal)) ?? Enumerable.Empty<IPublishedContent>());
        }
        public IEnumerable<IPublishedContent?> GetNodesInherit(IPublishedContent? content, ISet<string> aliases)
        {
            return content?.AncestorsOrSelf().Where(i => !aliases.Any() || aliases.Contains(i.ContentType.Alias)) ?? Enumerable.Empty<IPublishedContent>();
        }
        public IPublishedContent? GetContentPicker(IPublishedContent? content, string alias)
        {
            IPublishedContent? node = null;
            var nodeList = GetNodeList(content, alias);
            if (nodeList.Any())
            {
                node = nodeList?.FirstOrDefault();
            }
            return node;
        }
        /// <summary>
        /// Get recursive parent node where alias boolean value is true.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public IPublishedContent? GetNodeTrueBoolInherit(IPublishedContent content, string alias = "")
        {
            if (!string.IsNullOrEmpty(alias))
            {
                var node = content.AncestorsOrSelf()
                .Where(i => i.HasProperty(alias) && GetNodeBoolean(i, alias) && i.Level <= content.Level)
                .OrderByDescending(i => i.Level)
                .FirstOrDefault();

                if (NodeExists(node))
                {
                    return node;
                }
            }

            return null;
        }

        public string GetContentColor(IPublishedContent content, string alias, string defaultColor = null)
        {
            var colorModel = GetContentColorModel(content, alias, defaultColor);
            return colorModel.ColorValue;
        }

        public ColorPickerModel GetContentColorModel(IPublishedContent content, string alias, string defaultColor = null)
        {
            var color = !string.IsNullOrEmpty(GetContentValue(content, alias)) ? GetContentValue(content, alias) : null;
            ColorPickerModel? colorModel = new ColorPickerModel();

            if (color != null && color.StartsWith("{"))
            {
                colorModel = JsonConvert.DeserializeObject<ColorPickerModel>(color);
            }
            else if (color != null)
            {
                colorModel.ColorValue = color;
            }

            if (!string.IsNullOrEmpty(colorModel?.ColorValue) && !colorModel.ColorValue.StartsWith("#"))
            {
                colorModel.ColorValue = $"#{colorModel.ColorValue}";
            }
            if (string.IsNullOrEmpty(colorModel?.ColorValue) && !string.IsNullOrEmpty(defaultColor) && colorModel != null)
            {
                colorModel.ColorValue = defaultColor;
            }

            if (string.IsNullOrEmpty(colorModel?.ColorLabel) && colorModel != null)
            {
                colorModel.ColorLabel = colorModel.ColorValue;
            }

            return colorModel ?? new ColorPickerModel();
        }
    }
}

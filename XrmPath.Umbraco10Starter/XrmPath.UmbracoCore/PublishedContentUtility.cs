﻿using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using System.Data;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using XrmPath.Helpers.Model;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Models;
using XrmPath.UmbracoCore.Models.Definitions;

namespace XrmPath.UmbracoCore.Utilities
{
    public class PublishedContentUtility: BaseInitializer
    {
        public PublishedContentUtility(ServiceUtility? serviceUtil) : base(serviceUtil){}
        private readonly object LockExcludeNodesLookup = new object();
        private List<GenericModel>? _ExcludeNodeFolders { get; set; }

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
                loggingUtil?.Error($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentValue() for DocumentTypeAlias: {propertyAlias}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
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
                        strUrl = urlUtil?.UrlPickerLink(content, alias, "Url");
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
                    strTarget = urlUtil?.UrlPickerLink(content, alias, "Target") ?? "";
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
                loggingUtil?.Error($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetNodeList(). Content Value: {contentValue}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return nodeList;
        }

        public IEnumerable<IPublishedContent?> GetNodeList(ISet<int> intList)
        {
            if (intList.Any() && umbracoHelper != null)
            {
                var nodeList = intList.Select(i => umbracoHelper.Content(i)).Where(i => NodeExists(i));
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

        public string GetContentColor(IPublishedContent content, string alias, string? defaultColor = null)
        {
            var colorModel = GetContentColorModel(content, alias, defaultColor);
            return colorModel.ColorValue;
        }

        public ColorPickerModel GetContentColorModel(IPublishedContent content, string alias, string? defaultColor = null)
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
        public bool NodeHidden(IPublishedContent content, string alias = "umbracoNaviHide")
        {
            if (!content.HasProperty(alias))
            {
                //only check if hidden if property exists.
                return false;
            }
            var hidden = (!string.IsNullOrEmpty(GetContentValue(content, alias)) && GetContentValue(content, alias) == "1") || IsExcludedContent(content);
            return hidden;
        }

        public bool NodeHiddenFromSearch(IPublishedContent content, string alias = "hideFromSearch")
        {
            if (!content.HasProperty(alias))
            {
                //only check if hidden if property exists.
                return false;
            }
            var hidden = (!string.IsNullOrEmpty(GetContentValue(content, alias)) && GetContentValue(content, alias) == "1") || IsExcludedContent(content);
            return hidden;
        }
        /// <summary>
        /// Nodes will be excluded and hidden in NodeHidden and NodeHiddenFromSearch methods
        /// Test folder should be excluded from site completely
        /// Id represents Node ID, ValueInt represents level
        /// </summary>
        public List<GenericModel> ExcludeNodeFolders
        {
            get
            {
                if (_ExcludeNodeFolders == null)
                {
                    lock (LockExcludeNodesLookup)
                    {
                        if (_ExcludeNodeFolders == null)
                        {
                            //Exclude the Test Pages Folder
                            var excludeTestIds = queryUtil?.GetPageByUniqueId(UmbracoCustomLookups.TestPages);
                            if (excludeTestIds != null)
                            {
                                var testPagesFolder = new GenericModel { Id = excludeTestIds.Id, ValueInt = excludeTestIds.Level };
                                _ExcludeNodeFolders = new List<GenericModel> { testPagesFolder };
                            }
                            else
                            {
                                _ExcludeNodeFolders = new List<GenericModel>();
                            }
                        }
                    }
                }
                return _ExcludeNodeFolders;
            }
        }

        public bool IsExcludedContent(IPublishedContent content)
        {
            var excludedFolderIds = ExcludeNodeFolders;
            foreach (var excludedFolder in excludedFolderIds)
            {
                if (content.Level >= excludedFolder.ValueInt)
                {
                    var contentFolder = content.AncestorOrSelf(excludedFolder.ValueInt);
                    if (contentFolder != null && contentFolder.Id == excludedFolder.Id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasAccess(IPublishedContent content) { return true; }
        //public bool HasAccess(IPublishedContent content)
        //{
        //    var hasAccess = true;
        //    var isProtected = ServiceUtility.PublicAccessService.IsProtected(content.Path);
        //    if (isProtected)
        //    {
        //        hasAccess = false;
        //        if (MembershipHelper.UserIsAuthenticated())
        //        {
        //            try
        //            {
        //                hasAccess = ServiceUtility.PublicAccessService.HasAccess(content.Path, Membership.GetUser(), Roles.Provider);

        //            }
        //            catch (Exception ex)
        //            {
        //                //Serilog.Log.Warning($"XrmPath.UmbracoCore caught error on PublishedContentUtility.HasAccess(): {ex}");
        //                LogHelper.Warning($"XrmPath.UmbracoCore caught error on PublishedContentUtility.HasAccess(): {ex}");
        //            }
        //        }
        //    }
        //    return hasAccess;
        //}

        public int FindChildNodeId(IPublishedContent content, ISet<string> nodeTypeAliasSet)
        {
            var firstChildNode = FindChildNode(content, nodeTypeAliasSet)?.Id ?? 0;
            return firstChildNode;
        }
        public int FindChildNodeId(IPublishedContent content, string nodeTypeAlias)
        {
            var firstChildNode = FindChildNode(content, nodeTypeAlias)?.Id ?? 0;
            return firstChildNode;
        }
        public IPublishedContent? FindChildNode(IPublishedContent content, ISet<string> nodeTypeAliasSet)
        {
            if (content == null || content.Id == 0 || NodeHidden(content) || !HasAccess(content) || content.Children == null) return null;
            return content?.Children?.FirstOrDefault(child => NodeExists(child) && !NodeHidden(child) && nodeTypeAliasSet.Contains(child.ContentType.Alias));
        }
        public IPublishedContent? FindChildNode(IPublishedContent content, string nodeTypeAlias)
        {
            if (content == null || content.Id == 0 || NodeHidden(content) || !HasAccess(content) || content.Children == null) return null;
            return content.Children.FirstOrDefault(child => NodeExists(child) && !NodeHidden(child) && HasAccess(content) && string.Equals(nodeTypeAlias, child.ContentType.Alias, StringComparison.Ordinal));
        }

        public int FindContainerNodeId(IPublishedContent content, string nodeTypeAlias = "")
        {
            var containerId = FindContainerNode(content, nodeTypeAlias)?.Id ?? 0;
            return containerId;
        }

        public int FindContainerNodeId(IPublishedContent content, ISet<string> nodeTypeAliases)
        {
            var containerId = FindContainerNode(content, nodeTypeAliases)?.Id ?? 0;
            return containerId;
        }

        public IPublishedContent? FindContainerNode(IPublishedContent content, string alias)
        {
            var containerNode = GetNodesInherit(content, alias).FirstOrDefault();
            return containerNode;
        }

        public IPublishedContent? FindContainerNode(IPublishedContent content, ISet<string> aliases)
        {
            var containerNode = GetNodesInherit(content, aliases).FirstOrDefault();
            return containerNode;
        }

        public IEnumerable<IPublishedContent> FindAllNodes(IPublishedContent content, ISet<string> nodeTypeAliasSet, bool includeHidden = false)
        {
            if (content == null || content.Id == 0)
            {
                return Enumerable.Empty<IPublishedContent>();
            }
            var allNodes = content.DescendantsOrSelf().Where(i => nodeTypeAliasSet.Contains(i.ContentType.Alias) && (includeHidden || !NodeHidden(i)) && HasAccess(i));
            return allNodes;
        }

        public IEnumerable<IPublishedContent> FindAllNodes(IPublishedContent content, string nodeTypeAlias, bool includeHidden = false)
        {
            if (content == null || content.Id == 0)
            {
                return Enumerable.Empty<IPublishedContent>();
            }
            var allNodes = content.DescendantsOrSelf().Where(i => string.Equals(nodeTypeAlias, i.ContentType.Alias, StringComparison.Ordinal) && (includeHidden || !NodeHidden(i)) && HasAccess(i));
            return allNodes;
        }

        public string GetDate(IPublishedContent content, string dateFormat = "", string alias = "date")
        {
            var date = GetDateTime(content, alias);
            //if (string.IsNullOrEmpty(dateFormat))
            //{
            //    dateFormat = ConfigurationManager.AppSettings["dateFormat"];
            //}
            var strDate = date.ToString(dateFormat);
            return strDate;
        }

        public DateTime GetDateTime(IPublishedContent content, string alias = "date")
        {
            var date = content.CreateDate;
            var dateValue = GetContentValue(content, alias);
            if (!string.IsNullOrEmpty(dateValue))
            {
                date = Convert.ToDateTime(dateValue);
            }
            return date;
        }

        public DateTime? GetNullableDateTime(IPublishedContent content, string alias = "date", DateTime? defaultDate = null)
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
    }
}

using XrmPath.Helpers.Model;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Models;
using Newtonsoft.Json;
using Exception = System.Exception;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
//using Umbraco.Extensions;
using System.Net.NetworkInformation;
using Umbraco.Cms.Core;


namespace XrmPath.UmbracoCore.Utilities
{

    public static class PublishedContentUtility
    {


        private static readonly object LockExcludeNodesLookup = new object();
        private static List<GenericModel> _ExcludeNodeFolders { get; set; } = new List<GenericModel>();

        /// <summary>
        /// Nodes will be excluded and hidden in NodeHidden and NodeHiddenFromSearch methods
        /// Test folder should be excluded from site completely
        /// Id represents Node ID, ValueInt represents level
        /// </summary>
        public static List<GenericModel> ExcludeNodeFolders
        {
            get
            {
                //if (_ExcludeNodeFolders == null)
                //{
                //    lock (LockExcludeNodesLookup)
                //    {
                //        if (_ExcludeNodeFolders == null)
                //        {
                //            Exclude the Test Pages Folder
                //            var excludeTestIds = QueryUtility.GetPageByUniqueId(UmbracoCustomLookups.TestPages);
                //            if (excludeTestIds != null)
                //            {
                //                var testPagesFolder = new GenericModel { Id = excludeTestIds.Id, ValueInt = excludeTestIds.Level };
                //                _ExcludeNodeFolders = new List<GenericModel> { testPagesFolder };
                //            }
                //            else
                //            {
                //                _ExcludeNodeFolders = new List<GenericModel>();
                //            }
                //        }
                //    }
                //}
                _ExcludeNodeFolders = new List<GenericModel>();
                return _ExcludeNodeFolders;
            }
        }
        
        public static bool NodeExists(this IPublishedContent content)
        {
            if (content != null && content.Id > 0)
            {
                return true;
            }
            return false;
        }

        public static string GetContentValue(this IPublishedContent content, string propertyAlias, string defaultValue = "")
        {
            var result = defaultValue;
            if (string.IsNullOrEmpty(propertyAlias))
            {
                return result;
            }
            try
            {
                if (content.NodeExists())
                {
                    var property = content.GetProperty(propertyAlias);
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

        public static string GetContentValue(this IPublishedContent content, ISet<string> propertyAliases, string defaultValue = "")
        {
            var result = defaultValue;
            try
            {
                if (content.NodeExists())
                {
                    foreach (var propertyAlias in propertyAliases)
                    {
                        var hasValue = content.GetProperty(propertyAlias)?.HasValue() ?? false;
                        if (hasValue)
                        {
                            var property = content.GetProperty(propertyAlias);
                            if (property != null && property.HasValue() && !string.IsNullOrEmpty(property.GetValue()?.ToString()))
                            {
                                result = property.GetValue()?.ToString();
                            }
                            if (!string.IsNullOrEmpty(result))
                            {
                                return result;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //var aliases = string.Join(",", propertyAliases);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentValue() for DocumentTypeAliases: {aliases}. URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentValue() for DocumentTypeAliases: {aliases}. URL Info: {UrlUtility.GetCurrentUrl()}", ex);
                Console.WriteLine(ex.Message);
            }
            return result ?? String.Empty;
        }

        public static string GetTitle(this IPublishedContent content, string aliases = "title,pageTitle,name")
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
            return strTitle ?? String.Empty;
        }

        public static string GetDescription(this IPublishedContent content, string aliases = "")
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
            return desc ?? String.Empty;
        }

        public static string GetIcon(this IPublishedContent content, string aliases = "")
        {
            var icon = string.Empty;
            if (string.IsNullOrEmpty(aliases))
            {
                aliases = $"{UmbracoCustomFields.Icon}";
            }
            if (aliases.Contains(","))
            {
                var aliasList = aliases.StringToSet();
                foreach (var alias in aliasList)
                {
                    icon = content.GetContentValue(alias);
                    if (!string.IsNullOrEmpty(icon))
                    {
                        return icon ?? String.Empty;
                    }
                }
            }
            else
            {
                icon = content.GetContentValue(aliases);
            }
            return icon ?? String.Empty;
        }

        public static string GetImageIcon(this IPublishedContent content, string aliases = "")
        {
            var imageIcon = string.Empty;
            if (string.IsNullOrEmpty(aliases))
            {
                aliases = $"{UmbracoCustomFields.ImageIcon}";
            }


            if (aliases.Contains(","))
            {
                var aliasList = aliases.StringToSet();
                foreach (var alias in aliasList)
                {
                    //var imageItem = content.GetMediaItem(alias);
                    //imageIcon = imageItem?.Url ?? string.Empty;
                    var imageItem = string.Empty;
                    if (!string.IsNullOrEmpty(imageIcon))
                    {
                        return imageIcon;
                    }
                }
            }
            else
            {
                //var imageItem = content.GetMediaItem(aliases);
                //imageIcon = imageItem?.Url ?? string.Empty;
                var imageItem = string.Empty;
                if (!string.IsNullOrEmpty(imageIcon))
                {
                    return imageIcon;
                }
            }

            return imageIcon ?? String.Empty;
        }

        public static string GetImageIconStyle(this IPublishedContent content, string aliases = "")
        {
            var imageIconStyle = string.Empty;
            if (string.IsNullOrEmpty(aliases))
            {
                aliases = $"{UmbracoCustomFields.ImageIconStyle}";
            }
            if (aliases.Contains(","))
            {
                var aliasList = aliases.StringToSet();
                foreach (var alias in aliasList)
                {
                    imageIconStyle = content.GetContentValue(alias);
                    if (!string.IsNullOrEmpty(imageIconStyle))
                    {
                        return imageIconStyle;
                    }
                }
            }
            else
            {
                imageIconStyle = content.GetContentValue(aliases);
            }
            return imageIconStyle ?? String.Empty;
        }

        public static bool NodeHidden(this IPublishedContent content, string alias = "umbracoNaviHide")
        {
            var hasValue = content.GetProperty(alias)?.HasValue() ?? false;
            if (!hasValue)
            {
                //only check if hidden if property exists.
                return false;
            }
            var hidden = (!string.IsNullOrEmpty(content.GetContentValue(alias)) && content.GetContentValue(alias) == "1") || content.IsExcludedContent();
            return hidden;
        }

        public static bool NodeHiddenFromSearch(this IPublishedContent content, string alias = "hideFromSearch")
        {
            var hasValue = content.GetProperty(alias)?.HasValue() ?? false;
            if (!hasValue)
            {
                //only check if hidden if property exists.
                return false;
            }
            var hidden = (!string.IsNullOrEmpty(content.GetContentValue(alias)) && content.GetContentValue(alias) == "1") || content.IsExcludedContent();
            return hidden;
        }

        public static bool IsExcludedContent(this IPublishedContent content)
        {
            var excludedFolderIds = ExcludeNodeFolders;
            foreach (var excludedFolder in excludedFolderIds)
            {
                if (content.Level >= excludedFolder.ValueInt)
                {
                    //var contentFolder = content.AncestorOrSelf(excludedFolder.ValueInt);
                    //if (contentFolder != null && contentFolder.Id == excludedFolder.Id)
                    //{
                    //    return true;
                    //}
                }
            }
            return false;
        }

        public static bool HasAccess(this IPublishedContent content)
        {
            var hasAccess = true;
            //var isProtected = ServiceUtility.PublicAccessService.IsProtected(content.Path);
            //if (isProtected)
            //{
            //    hasAccess = false;
            //    if (MembershipHelper.UserIsAuthenticated())
            //    {
            //        try
            //        {
            //            hasAccess = ServiceUtility.PublicAccessService.HasAccess(content.Path, Membership.GetUser(), Roles.Provider);

            //        }
            //        catch (Exception ex)
            //        {
            //            //Serilog.Log.Warning($"XrmPath.UmbracoCore caught error on PublishedContentUtility.HasAccess(): {ex}");
            //            LogHelper.Warning($"XrmPath.UmbracoCore caught error on PublishedContentUtility.HasAccess(): {ex}");
            //        }
            //    }
            //}
            return hasAccess;
        }

        public static int FindChildNodeId(this IPublishedContent content, ISet<string> nodeTypeAliasSet)
        {
            var firstChildNode = content.FindChildNode(nodeTypeAliasSet)?.Id ?? 0;
            return firstChildNode;
        }
        public static int FindChildNodeId(this IPublishedContent content, string nodeTypeAlias)
        {
            var firstChildNode = content.FindChildNode(nodeTypeAlias)?.Id ?? 0;
            return firstChildNode;
        }
        public static IPublishedContent? FindChildNode(this IPublishedContent content, ISet<string> nodeTypeAliasSet)
        {
            if (content == null || content.Id == 0 || content.NodeHidden() || !content.HasAccess() || content?.Children == null) return null;
            return content.Children.FirstOrDefault(child => child.NodeExists() && !child.NodeHidden() && nodeTypeAliasSet.Contains(child.ContentType.Alias));
        }
        public static IPublishedContent? FindChildNode(this IPublishedContent content, string nodeTypeAlias)
        {
            if (content == null || content.Id == 0 || content.NodeHidden() || !content.HasAccess() || content.Children == null) return null;
            return content.Children.FirstOrDefault(child => child.NodeExists() && !child.NodeHidden() && content.HasAccess() && string.Equals(nodeTypeAlias, child.ContentType.Alias, StringComparison.Ordinal));
        }

        //public static int FindContainerNodeId(this IPublishedContent content, string nodeTypeAlias = "")
        //{
        //    var containerId = content.FindContainerNode(nodeTypeAlias)?.Id ?? 0;
        //    return containerId;
        //}

        //public static int FindContainerNodeId(this IPublishedContent content, ISet<string> nodeTypeAliases)
        //{
        //    var containerId = content.FindContainerNode(nodeTypeAliases)?.Id ?? 0;
        //    return containerId;
        //}

        //public static IPublishedContent FindContainerNode(this IPublishedContent content, string alias)
        //{
        //    var containerNode = content.GetNodesInherit(alias).FirstOrDefault();
        //    return containerNode;
        //}

        //public static IPublishedContent FindContainerNode(this IPublishedContent content, ISet<string> aliases)
        //{
        //    var containerNode = content.GetNodesInherit(aliases).FirstOrDefault();
        //    return containerNode;
        //}

        //public static IEnumerable<IPublishedContent> FindAllNodes(this IPublishedContent content, ISet<string> nodeTypeAliasSet, bool includeHidden = false)
        //{
        //    if (content == null || content.Id == 0)
        //    {
        //        return Enumerable.Empty<IPublishedContent>();
        //    }
        //    var allNodes = content.DescendantsOrSelf().Where(i => nodeTypeAliasSet.Contains(i.ContentType.Alias) && (includeHidden || !i.NodeHidden()) && i.HasAccess());
        //    return allNodes;
        //}

        //public static IEnumerable<IPublishedContent> FindAllNodes(this IPublishedContent content, string nodeTypeAlias, bool includeHidden = false)
        //{
        //    if (content == null || content.Id == 0)
        //    {
        //        return Enumerable.Empty<IPublishedContent>();
        //    }
        //    var allNodes = content.DescendantsOrSelf().Where(i => string.Equals(nodeTypeAlias, i.ContentType.Alias, StringComparison.Ordinal) && (includeHidden || !i.NodeHidden()) && i.HasAccess());
        //    return allNodes;
        //}

        public static string GetDate(this IPublishedContent content, string dateFormat = "", string alias = "date")
        {
            var date = GetDateTime(content, alias);
            if (string.IsNullOrEmpty(dateFormat))
            {
                //dateFormat = ConfigurationManager.AppSettings["dateFormat"];
            }
            var strDate = date.ToString(dateFormat);
            return strDate;
        }

        public static DateTime GetDateTime(this IPublishedContent content, string alias = "date")
        {
            var date = content.CreateDate;
            var dateValue = content.GetContentValue(alias);
            if (!string.IsNullOrEmpty(dateValue))
            {
                date = Convert.ToDateTime(dateValue);
            }
            return date;
        }

        public static DateTime? GetNullableDateTime(this IPublishedContent content, string alias = "date", DateTime? defaultDate = null)
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

        public static string GetUrl(this IPublishedContent content, string alias = "urlPicker")
        {
            var strUrl = "";
            if (content.NodeExists())
            {
                //var reverseProxyFolder = SiteUrlUtility.GetRootFromReverseProxy();
                var nodeUrl = content.NodeUrl();
                strUrl = nodeUrl;
                
                if (!string.IsNullOrEmpty(alias))
                { 
                    //check URL Property
                    var stringData = content.GetContentValue(alias);
                    if (!string.IsNullOrWhiteSpace(stringData))
                    {
                        //strUrl = MultiUrlUtility.UrlPickerLink(content, alias, "Url");
                        strUrl = "";
                        if (string.IsNullOrWhiteSpace(strUrl))
                        {
                            strUrl = nodeUrl;
                        }
                    }
                }
            }
            return strUrl;
        }

        public static string NodeUrl(this IPublishedContent content)
        {
            var nodeUrl = "";
            if (content.NodeExists())
            {
                //nodeUrl = SiteUrlHelper.GetSiteUrl(content.Url);
                nodeUrl = "";
            }
            return nodeUrl;
        }
        
        public static string GetFullUrl(this IPublishedContent content, string alias = "urlPicker")
        {
            var nodeUrl = GetUrl(content, alias);
            //var url = SiteUrlHelper.GetFullUrl(nodeUrl);
            var url = "";
            return url;
        }


        public static string GetTarget(this IPublishedContent content, string alias = "urlPicker")
        {
            string strTarget = "_self";
            if (content.NodeExists())
            {
                //check URL Property
                var stringData = content.GetContentValue(alias);
                if (!string.IsNullOrWhiteSpace(stringData))
                {
                    //strTarget = MultiUrlUtility.UrlPickerLink(content, alias, "Target");
                    strTarget = "_self";
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

        public static int GetContentPickerId(this IPublishedContent content, string alias)
        {
            var intValue = 0;
            var nodeList = content.GetNodeList(alias);
            if (nodeList.Any())
            {
                intValue = nodeList.First().Id;
            }

            return intValue;
        }

        public static int GetNodeInt(this IPublishedContent content, string alias)
        {
            var intValue = 0;
            var nodeValue = content.GetContentValue(alias);
            if (!string.IsNullOrEmpty(nodeValue))
            {
                int.TryParse(nodeValue, out intValue);
            }

            return intValue;
        }

        public static IPublishedContent? GetContentPicker(this IPublishedContent content, string alias)
        {
            IPublishedContent? node = null;
            var nodeList = content.GetNodeList(alias);
            if (nodeList.Any())
            {
                node = nodeList.FirstOrDefault();
            }
            return node;
        }

        public static decimal GetNodeDecimal(this IPublishedContent content, string alias, decimal defaultValue = 0)
        {
            var decValue = defaultValue;
            var contentValue = content.GetContentValue(alias);

            if (!string.IsNullOrEmpty(contentValue))
            {
                decimal.TryParse(contentValue, out decValue);
            }
            return decValue;
        }

        public static double GetNodeDouble(this IPublishedContent content, string alias, double defaultValue = 0)
        {
            var dbValue = defaultValue;
            var contentValue = content.GetContentValue(alias);

            if (!string.IsNullOrEmpty(contentValue))
            {
                double.TryParse(contentValue, out dbValue);
            }
            return dbValue;
        }

        public static bool IsMembersEntryPage(this IPublishedContent content, string alias = "membersEntry")
        {
            var membersEntryValue = content.GetContentValue(alias);
            var isEntry = !string.IsNullOrEmpty(membersEntryValue) && membersEntryValue == "1";
            return isEntry;
        }

        public static bool GetNodeBoolean(this IPublishedContent content, string alias, bool? defaultBoolean = null)
        {
            bool boolValue;
            var contentValue = content.GetContentValue(alias);

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

        public static IPublishedContent? GetNodeFromString(string contentValue)
        {
            if (contentValue.Contains("umb://"))
            {
                var nodeList = new List<IPublishedContent>();
                var udiValue = contentValue.Split(',').First();
                //Udi udi;
                //var validUdi = Udi.TryParse(udiValue, out udi);
                var udi = Udi.Create(udiValue);
                var validUdi = true;
                if (validUdi && udi != null)
                {
                    //var contentPicker = ServiceUtility.UmbracoHelper.Content(udi);
                    //if (contentPicker.NodeExists())
                    //{
                    //    return contentPicker;
                    //}
                }
            }
            else
            {
                var nodeList = new List<IPublishedContent>();
                var idValue = contentValue.Split(',').First();
                int id;
                var validId = int.TryParse(idValue, out id);
                if (validId && id > 0)
                {
                    //var node = ServiceUtility.UmbracoHelper.GetById(id);
                    //if (node.NodeExists())
                    //{
                    //    return node;
                    //}
                }
            }
            return null;
        }

        public static List<IPublishedContent> GetNodeList(this IPublishedContent content, string alias)
        {
            var nodeList = new List<IPublishedContent>();
            var contentValue = content.GetContentValue(alias);

            try
            {
                if (!string.IsNullOrEmpty(contentValue))
                {
                    //var multiNode = content.Value<IEnumerable<IPublishedContent>>(alias);
                    //IPublishedContent singleNode = null;

                    //if (multiNode == null)
                    //{
                    //    singleNode = content.Value<IPublishedContent>(alias);
                    //    nodeList.Add(singleNode);
                    //}
                    //else {
                    //    nodeList = multiNode.ToList();
                    //}

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

        public static IEnumerable<IPublishedContent> GetNodeList(this ISet<int> intList)
        {
            if (intList.Any())
            {
                //return intList.Select(i => ServiceUtility.UmbracoHelper.GetById(i)).Where(i => i.NodeExists());
            }
            return Enumerable.Empty<IPublishedContent>();
        }

        //public static IEnumerable<int> GetNodeIdsInherit(this IPublishedContent content, string alias)
        //{
        //    var nodeListIds = GetNodesInherit(content).SelectMany(i => i.GetNodeIdsSingle(alias));
        //    var returnIds = nodeListIds.Distinct();
        //    return returnIds;
        //}

        public static IEnumerable<int> GetNodeIdsSingle(this IPublishedContent content, string alias)
        {
            var nodeList = content.GetNodeList(alias);
            var nodeIds = nodeList.Select(i => i.Id);
            return nodeIds;
        }

        /// <summary>
        /// Gets recursive parents boolean value for a specified alias
        /// </summary>
        /// <param name="content"></param>
        /// <param name="alias"></param>
        /// <param name="defaultBool"></param>
        /// <returns></returns>
        //public static bool GetBoolInherit(this IPublishedContent content, string alias = "", bool defaultBool = false)
        //{

        //    var boolInherit = defaultBool;

        //    if (!string.IsNullOrEmpty(alias))
        //    { 
        //        var node = content.AncestorsOrSelf().Where(i => i.HasProperty(alias) && !string.IsNullOrEmpty(i.GetContentValue(alias)))
        //        .OrderByDescending(i => i.Level)
        //        .FirstOrDefault();

        //        if (node != null && node.NodeExists())
        //        {
        //            boolInherit = node.GetNodeBoolean(alias);
        //        }
        //    }

        //    return boolInherit;
        //}

        /// <summary>
        /// Get recursive parent node where alias boolean value is true.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        //public static IPublishedContent GetNodeTrueBoolInherit(this IPublishedContent content, string alias = "")
        //{
        //    if (!string.IsNullOrEmpty(alias))
        //    { 
        //        var node = content.AncestorsOrSelf()
        //        .Where(i => i.HasProperty(alias) && i.GetNodeBoolean(alias) && i.Level <= content.Level)
        //        .OrderByDescending(i => i.Level)
        //        .FirstOrDefault();

        //        if (node != null && node.NodeExists())
        //        {
        //            return node;
        //        }
        //    }

        //    return null;
        //}

        //public static IEnumerable<IPublishedContent> GetNodesInherit(this IPublishedContent content, string alias = "")
        //{
        //    return content.AncestorsOrSelf().Where(i => string.IsNullOrEmpty(alias) || string.Equals(alias, i.ContentType.Alias, StringComparison.Ordinal));
        //}
        //public static IEnumerable<IPublishedContent> GetNodesInherit(this IPublishedContent content, ISet<string> aliases)
        //{
        //    return content.AncestorsOrSelf().Where(i => !aliases.Any() || aliases.Contains(i.ContentType.Alias));
        //}

        //public static IEnumerable<GenericModel> GetContentLookupList(string dataTypeName)
        //{
        //    var lookupList = Enumerable.Empty<GenericModel>();
        //    try
        //    {
        //        var dataTypeDefn = ServiceUtility.DataTypeService.GetAll().First(i => i.Name == dataTypeName);
        //        if (dataTypeDefn?.Id > 0)
        //        {
        //            lookupList = ServiceUtility.DataTypeService.GetAll(dataTypeDefn.Id).Select(f => new GenericModel
        //            {
        //                Id = f.Id,
        //                Text = f.Name,
        //                Value = f.Key.ToString()
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Serilog.Log.Warning($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentLookupList(). Cannot get lookup list for data type '{dataTypeName}'. URL Info: {UrlUtility.GetCurrentUrl()}. Error Message: {ex}");
        //        //LogHelper.Warning($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentLookupList(). Cannot get lookup list for data type '{dataTypeName}'. URL Info: {UrlUtility.GetCurrentUrl()}. Error Message: {ex}");
        //    }

        //    return lookupList;
        //}

        //public static int GetContentLookupId(string dataTypeName, string lookupText)
        //{
        //    var preValueId = 0;
        //    var errorMessage = $"XrmPath.Web caught error on PublishedContentUtility.GetContentLookupId(). Cannot find lookup '{lookupText}' for data type '{dataTypeName}'";
        //    try
        //    {
        //        var dataTypeDefn = ServiceUtility.DataTypeService.GetAll().First(x => x.Name.Trim().ToLower() == dataTypeName.Trim().ToLower());
        //        if (dataTypeDefn?.Id > 0)
        //        {
        //            var lookupItem = ServiceUtility.DataTypeService.GetAll(dataTypeDefn.Id).Where(d => d.Name.Equals(lookupText)).ToList();
        //            if (lookupItem.Any())
        //            {
        //                preValueId = lookupItem.Select(f => f.Id).First();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Serilog.Log.Warning($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentLookupId ({errorMessage}). URL Info: {UrlUtility.GetCurrentUrl()}. Error Message: {ex}");
        //        LogHelper.Warning($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentLookupId ({errorMessage}). URL Info: {UrlUtility.GetCurrentUrl()}. Error Message: {ex}");
        //    }

        //    if (preValueId == 0)
        //    {
        //        //Serilog.Log.Warning($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentLookupId ({errorMessage})");
        //        LogHelper.Warning($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetContentLookupId ({errorMessage})");
        //    }

        //    return preValueId;
        //}

        //public static IEnumerable<IPublishedContent> GetTaggedContent(string doctypeAlias, string fieldAlias, string taggedValue)
        //{
        //    var taggedNodes = new List<IPublishedContent>();

        //    try
        //    {
        //        var fullWordNodes = QueryUtility.GetPublishedContentByType(doctypeAlias).Where(i => i.GetContentValue(fieldAlias).ToLower().Contains(taggedValue.ToLower())).ToList();
        //        taggedNodes.AddRange(fullWordNodes);

        //        if (taggedValue.IndexOf(" ", StringComparison.Ordinal) > 0)
        //        {
        //            var wordArray = taggedValue.Split(' ');
        //            foreach (var word in wordArray)
        //            {
        //                var partialWordNodes = QueryUtility.GetPublishedContentByType(doctypeAlias).Where(i => i.GetContentValue(fieldAlias).ToLower().Contains(word.ToLower())).ToList();
        //                var additionalNodes = partialWordNodes.Where(i => !taggedNodes.Select(x => x.Id).Contains(i.Id)).ToList();
        //                if (additionalNodes.Any())
        //                {
        //                    taggedNodes.AddRange(additionalNodes);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetTaggedContent(). URL Info: {UrlUtility.GetCurrentUrl()}");
        //        //LogHelper.Error($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetTaggedContent(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
        //    }

        //    return taggedNodes;
        //}

        public static List<string> GetTagList(this IPublishedContent content, string fieldAlias)
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
            return tagList ?? new List<string>();
        }

        public static string GetContentColor(this IPublishedContent content, string alias, string? defaultColor = null)
        {
            var colorModel = content.GetContentColorModel(alias, defaultColor);
            return colorModel.ColorValue;
        }

        public static ColorPickerModel GetContentColorModel(this IPublishedContent content, string alias, string? defaultColor = null)
        {
            var color = !string.IsNullOrEmpty(content.GetContentValue(alias)) ? content.GetContentValue(alias) : null;
            var colorModel = new ColorPickerModel();

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
            if (string.IsNullOrEmpty(colorModel?.ColorValue) && !string.IsNullOrEmpty(defaultColor))
            {
                colorModel.ColorValue = defaultColor;
            }

            if (string.IsNullOrEmpty(colorModel?.ColorLabel))
            {
                colorModel.ColorLabel = colorModel.ColorValue;
            }
            
            return colorModel;
        }

        //public static IPublishedContent GetContentByName(string alias, string name)
        //{
        //    if (!string.IsNullOrEmpty(alias) && !string.IsNullOrEmpty(name))
        //    {
        //        var contentList = QueryUtility.GetPublishedContentByType(alias).Where(i => i.Name.Equals(name)).ToList();
        //        if (contentList.Count == 1)
        //        {
        //            var content = contentList.SingleOrDefault();
        //            return content;
        //        }
        //    }
        //    return null;
        //}

        public static string GetUdiString (this IPublishedContent content)
        {
            var udi = Udi.Create(Constants.UdiEntityType.Document, content.Key).ToString();
            return udi;
        }
        //public static int GetNodeIdFromUdiString(string udiString)
        //{
        //    var udi = Udi.Parse(udiString);
        //    var content = ServiceUtility.UmbracoHelper.Content(udi);
        //    var id = content?.Id ?? 0;
        //    return id;
        //}

        public static int GetIdFromLink(this Link item, LinkType linkType)
        {
            //var nodeId = item?.Id ?? 0;
            var nodeId = 0;

            try
            {
                if (item?.Udi != null)
                {
                    if (linkType == LinkType.Content)
                    {
                        //var node = ServiceUtility.UmbracoHelper.Content(item.Udi);
                        //if (node != null && node.NodeExists())
                        //{
                        //    nodeId = node.Id;
                        //}
                    }
                    else if (linkType == LinkType.Media) 
                    {
                        //var node = ServiceUtility.UmbracoHelper.Media(item.Udi);
                        //if (node != null && node.NodeExists())
                        //{
                        //    nodeId = node.Id;
                        //}
                    }
                    
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetIdFromLink(). URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on PublishedContentUtility.GetIdFromLink(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return nodeId;
        }
    }
}
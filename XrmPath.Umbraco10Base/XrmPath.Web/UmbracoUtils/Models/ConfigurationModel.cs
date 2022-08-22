using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XrmPath.Helpers.Utilities;

namespace XrmPath.UmbracoUtils.Models
{
    public static class ConfigurationModel
    {

        public static readonly string WebsiteContentTypes = $"{UmbracoCustomTypes.HomePage},{UmbracoCustomTypes.ArticleList},{UmbracoCustomTypes.Article},{UmbracoCustomTypes.Link},{UmbracoCustomTypes.ContentPage},{UmbracoCustomTypes.SearchPage},{UmbracoCustomTypes.Sitemap}";
        public static List<string> WebsiteContentTypesList
        {
            get
            {
                return WebsiteContentTypes.Split(',').ToList();
            }
        }
        public static ISet<string> WebsiteContentTypesSet
        {
            get
            {
                return WebsiteContentTypes.StringToSet();
            }
        }

        //public static readonly string SearchableFields = $"title,bodyText,body,description,contentGrid,tags,metaName,metaDescription,metaKeywords,FileTextContent";
        public static readonly string SearchableFields = $"{UmbracoCustomFields.Title},{UmbracoCustomFields.BodyText},{UmbracoCustomFields.Description},{UmbracoCustomFields.Grid},{UmbracoCustomFields.Tags},{UmbracoCustomFields.MetaDescription},{UmbracoCustomFields.MetaKeywords},{UmbracoCustomFields.FileTextContent}," +
            $"{UmbracoCustomFields.UserName},{UmbracoCustomFields.UserComment}";
        public static List<string> SearchableFieldsList
        {
            get
            {
                return SearchableFields.Split(',').ToList();
            }
        }
        public static ISet<string> SearchableFieldsSet {
            get 
            {
                return SearchableFields.StringToSet();
            }
        }

        public static List<string> SearchableContentTypesList 
        {
            get 
            {
                var websiteContentTypeList = WebsiteContentTypesList;
                var excludeContentTypeList = new List<string> { UmbracoCustomTypes.HomePage, UmbracoCustomTypes.SearchPage };
                var searchableContentTypeList = websiteContentTypeList.Except(excludeContentTypeList).ToList();
                return searchableContentTypeList;
            }
        }

    }
}
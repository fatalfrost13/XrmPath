using Umbraco.Core.Services;
using Umbraco.Web;

namespace XrmPath.UmbracoCore.Utilities
{
    public static class ServiceUtility
    {
        //private static UmbracoHelper _umbracoHelper;
        public static UmbracoHelper UmbracoHelper
        {
            get
            {
                //if (_umbracoHelper == null || HttpContext.Current == null)
                //{
                //    _umbracoHelper = CustomUmbracoHelper.GetUmbracoHelper();
                //}
                //return _umbracoHelper;
                return Umbraco.Web.Composing.Current.UmbracoHelper;
            }
            //set
            //{
            //    _umbracoHelper = value;
            //}
        }

        //private static IContentService _contentService;
        public static IContentService ContentService
        {
            get
            {
                //if (_contentService == null)
                //{
                //    _contentService = Umbraco.Core.Composing.Current.Services.ContentService; //ApplicationContext.Current.Services.ContentService;
                //}
                //return _contentService;
                return Umbraco.Core.Composing.Current.Services.ContentService;
            }
            //set
            //{
            //    _contentService = value;
            //}
        }

        //private static IMemberService _memberService;
        public static IMemberService MemberService
        {
            get
            {
                //if (_memberService == null)
                //{
                //    _memberService = Umbraco.Core.Composing.Current.Services.MemberService;
                //}
                //return _memberService;
                return Umbraco.Core.Composing.Current.Services.MemberService;
            }
            //set
            //{
            //    _memberService = value;
            //}
        }

        //private static IDataTypeService _dataTypeService;
        public static IDataTypeService DataTypeService
        {
            get
            {
                //if (_dataTypeService == null)
                //{
                //    _dataTypeService = Umbraco.Core.Composing.Current.Services.DataTypeService;
                //}
                //return _dataTypeService;
                return Umbraco.Core.Composing.Current.Services.DataTypeService;
            }
            //set
            //{
            //    _dataTypeService = value;
            //}
        }

        //private static IPublicAccessService _publicAccessService;
        public static IPublicAccessService PublicAccessService
        {
            get
            {
                //if (_publicAccessService == null)
                //{
                //    _publicAccessService = Umbraco.Core.Composing.Current.Services.PublicAccessService;
                //}
                //return _publicAccessService;
                return Umbraco.Core.Composing.Current.Services.PublicAccessService;
            }
            //set
            //{
            //    _publicAccessService = value;
            //}
        }

        //private static IMediaService _mediaService;
        public static IMediaService MediaService
        {
            get
            {
                //if (_mediaService == null)
                //{
                //    _mediaService = Umbraco.Core.Composing.Current.Services.MediaService;
                //}
                //return _mediaService;
                return Umbraco.Core.Composing.Current.Services.MediaService;
            }
            //set
            //{
            //    _mediaService = value;
            //}
        }

        //private static IPackagingService _packagingService;
        public static IPackagingService PackagingService
        {
            get
            {
                //if (_packagingService == null)
                //{
                //    _packagingService = Umbraco.Core.Composing.Current.Services.PackagingService;
                //}
                //return _packagingService;
                return Umbraco.Core.Composing.Current.Services.PackagingService;
            }
            //set
            //{
            //    _packagingService = value;
            //}
        }

        //private static IUserService _userService;
        public static IUserService UserService
        {
            get
            {
                //if (_userService == null)
                //{
                //    _userService = Umbraco.Core.Composing.Current.Services.UserService;
                //}
                //return _userService;
                return Umbraco.Core.Composing.Current.Services.UserService;
            }
            //set
            //{
            //    _userService = value;
            //}
        }

        //private static IContentTypeService _contentTypeService;
        public static IContentTypeService ContentTypeService
        {
            get
            {
                //if (_contentTypeService == null)
                //{
                //    _contentTypeService = Umbraco.Core.Composing.Current.Services.ContentTypeService;
                //}
                //return _contentTypeService;
                return Umbraco.Core.Composing.Current.Services.ContentTypeService;
            }
            //set
            //{
            //    _contentTypeService = value;
            //}
        }
    }
}
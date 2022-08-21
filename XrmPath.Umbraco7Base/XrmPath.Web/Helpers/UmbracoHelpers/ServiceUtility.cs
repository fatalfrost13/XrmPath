using System.Web;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace XrmPath.Web.Helpers.UmbracoHelpers
{
    public static class ServiceUtility
    {
        private static UmbracoHelper _umbracoHelper;
        public static UmbracoHelper UmbracoHelper
        {
            get
            {
                if (_umbracoHelper == null || HttpContext.Current == null)
                {
                    _umbracoHelper = CustomUmbracoHelper.GetUmbracoHelper();
                }
                return _umbracoHelper;
            }
            set
            {
                _umbracoHelper = value;
            }
        }

        private static IContentService _contentService;
        public static IContentService ContentService
        {
            get
            {
                if (_contentService == null)
                {
                    _contentService = ApplicationContext.Current.Services.ContentService;
                }
                return _contentService;
            }
            set
            {
                _contentService = value;
            }
        }

        private static IMemberService _memberService;
        public static IMemberService MemberService
        {
            get
            {
                if (_memberService == null)
                {
                    _memberService = ApplicationContext.Current.Services.MemberService;
                }
                return _memberService;
            }
            set
            {
                _memberService = value;
            }
        }

        private static IDataTypeService _dataTypeService;
        public static IDataTypeService DataTypeService
        {
            get
            {
                if (_dataTypeService == null)
                {
                    _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                }
                return _dataTypeService;
            }
            set
            {
                _dataTypeService = value;
            }
        }

        private static IPublicAccessService _publicAccessService;
        public static IPublicAccessService PublicAccessService
        {
            get
            {
                if (_publicAccessService == null)
                {
                    _publicAccessService = ApplicationContext.Current.Services.PublicAccessService;
                }
                return _publicAccessService;
            }
            set
            {
                _publicAccessService = value;
            }
        }

        private static IMediaService _mediaService;
        public static IMediaService MediaService
        {
            get
            {
                if (_mediaService == null)
                {
                    _mediaService = ApplicationContext.Current.Services.MediaService;
                }
                return _mediaService;
            }
            set
            {
                _mediaService = value;
            }
        }

        private static IPackagingService _packagingService;
        public static IPackagingService PackagingService
        {
            get
            {
                if (_packagingService == null)
                {
                    _packagingService = ApplicationContext.Current.Services.PackagingService;
                }
                return _packagingService;
            }
            set
            {
                _packagingService = value;
            }
        }

        private static IUserService _userService;
        public static IUserService UserService
        {
            get
            {
                if (_userService == null)
                {
                    _userService = ApplicationContext.Current.Services.UserService;
                }
                return _userService;
            }
            set
            {
                _userService = value;
            }
        }

        private static IContentTypeService _contentTypeService;
        public static IContentTypeService ContentTypeService
        {
            get
            {
                if (_contentTypeService == null)
                {
                    _contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                }
                return _contentTypeService;
            }
            set
            {
                _contentTypeService = value;
            }
        }
    }
}
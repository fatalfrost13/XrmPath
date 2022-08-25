using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Security;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.BaseServices
{
    public abstract class BaseUtility
    {
        protected readonly UmbracoHelper? _umbracoHelper;
        protected readonly ILogger<object>? _logger;
        protected readonly IMediaService? _mediaService;
        protected readonly IExamineManager? _examineManager;
        protected readonly IContentService? _contentService;
        protected readonly IContentTypeService? _contentTypeService;
        protected readonly AppSettingsModel? _appSettings;
        protected readonly IMemberManager? _memberManager;
        protected readonly IMemberSignInManager? _memberSignInManager;
        public BaseUtility(ILogger<object>? logger, UmbracoHelper? umbracoHelper, IMediaService? mediaService, IExamineManager? examineManager, IContentService? contentService, IContentTypeService? contentTypeService, IMemberManager? memberManager, IMemberSignInManager? memberSignInManager, IOptions<AppSettingsModel>? appSettings)
        {
            if (logger != null && _logger == null)
            {
                _logger = logger;
            }
            if (umbracoHelper != null && _umbracoHelper == null)
            {
                _umbracoHelper = umbracoHelper;
            }
            if (mediaService != null && _mediaService == null)
            {
                _mediaService = mediaService;
            }
            if (examineManager != null && _examineManager == null)
            {
                _examineManager = examineManager;
            }
            if (contentService != null && _contentService == null)
            {
                _contentService = contentService;
            }
            if (contentTypeService != null && _contentTypeService == null)
            {
                _contentTypeService = contentTypeService;
            }
            if (appSettings != null && _appSettings == null)
            {
                _appSettings = appSettings?.Value;
            }
            if (memberManager != null && _memberManager == null)
            {
                _memberManager = memberManager;
            }
            if (memberSignInManager != null && _memberSignInManager == null)
            {
                _memberSignInManager = memberSignInManager;
            }
        }
    }
}

using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.Utilities
{
    public abstract class BaseUtility
    {
        protected readonly UmbracoHelper? _umbracoHelper;
        protected readonly ILogger<object>? _iLogger;
        protected readonly IMediaService? _mediaService;
        protected readonly IExamineManager? _examineManager;
        protected readonly IContentService? _contentService;
        protected readonly IContentTypeService? _contentTypeService;
        protected readonly IOptions<AppSettingsModel>? _appSettings;
        public BaseUtility(ILogger<object>? iLogger, UmbracoHelper? umbracoHelper, IMediaService? mediaService, IExamineManager? examineManager, IContentService? contentService, IContentTypeService? contentTypeService, IOptions<AppSettingsModel>? appSettings)
        {
            if (iLogger != null && _iLogger == null)
            {
                _iLogger = iLogger;
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
                _appSettings = appSettings;
            }
        }
    }
}

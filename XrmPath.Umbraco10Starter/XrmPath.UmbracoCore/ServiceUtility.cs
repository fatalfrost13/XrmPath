using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.Utilities
{
    public class ServiceUtility : BaseUtility
    {
        protected MultiUrlUtility? _urlUtil;
        protected MediaUtility? _mediaUtil;
        protected ContentUtility? _contentUtil;
        protected LoggingUtility? _loggingUtil;
        protected PublishedContentUtility? _pcUtil;
        protected SearchUtility? _searchUtil;
        public ServiceUtility(ILogger<object>? iLogger = null, UmbracoHelper? umbracoHelper = null, IMediaService? mediaService = null, IExamineManager? examineManager = null, IContentService? contentService = null, IContentTypeService? contentTypeService = null, IOptions<AppSettingsModel>? appSettings = null)
            : base(iLogger, umbracoHelper, mediaService, examineManager, contentService, contentTypeService, appSettings)
        {
            if (_pcUtil == null) {
                _pcUtil = new PublishedContentUtility(this);
            }
        }

        public LoggingUtility? GetLoggingUtility()
        {
            if (_loggingUtil == null && _iLogger != null)
            {
                _loggingUtil = new LoggingUtility(_iLogger);
            }
            return _loggingUtil;
        }

        public PublishedContentUtility? GetPublishedContentUtility()
        {
            return _pcUtil;
        }
        public UmbracoHelper? GetUmbracoHelper()
        {
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
            if (_urlUtil == null && _umbracoHelper != null)
            {
                _urlUtil = new MultiUrlUtility(this);
            }
            return _urlUtil;
        }
        public ContentUtility? GetContentUtility()
        {
            if (_contentUtil == null && _contentService != null && _umbracoHelper != null && _contentTypeService != null)
            {
                _contentUtil = new ContentUtility(this);
            }
            return _contentUtil;
        }
        public MediaUtility? GetMediaUtility()
        {
            if (_mediaUtil == null && _mediaService != null && _umbracoHelper != null)
            {
                _mediaUtil = new MediaUtility(this);
            }
            return _mediaUtil;
        }
        public SearchUtility? GetSearchUtility()
        {
            if (_searchUtil == null && _mediaService != null && _umbracoHelper != null && _examineManager != null)
            {
                _searchUtil = new SearchUtility(this);
            }
            return _searchUtil;
        }
    }
}

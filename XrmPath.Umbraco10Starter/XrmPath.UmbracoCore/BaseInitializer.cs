using Examine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;

namespace XrmPath.UmbracoCore.Utilities
{

    public abstract class BaseInitializer
    {
        protected ServiceUtility? _serviceUtil;
        protected PublishedContentUtility? _pcUtil;
        protected MultiUrlUtility? _urlUtil;
        protected ContentUtility? _contentUtil;
        protected LoggingUtility? _loggingUtil;
        protected MediaUtility? _mediaUtil;
        protected QueryUtility? _queryUtil;

        protected UmbracoHelper? umbracoHelper;
        protected IExamineManager? examineManager;
        protected IMediaService? mediaService;
        protected IContentService? contentService;
        protected IContentTypeService? contentTypeService;

        public BaseInitializer(ServiceUtility? serviceUtil) {
            if (_serviceUtil == null && serviceUtil != null)
            {
                _serviceUtil = serviceUtil;
            }
            if (umbracoHelper == null)
            {
                umbracoHelper = _serviceUtil?.GetUmbracoHelper();
            }
            if (mediaService == null)
            {
                mediaService = _serviceUtil?.GetMediaService();
            }
            if (examineManager == null)
            {
                examineManager = _serviceUtil?.GetExamineManager();
            }
            if (contentService == null)
            {
                contentService = _serviceUtil?.GetContentService();
            }
            if (contentTypeService == null)
            {
                contentTypeService = _serviceUtil?.GetContentServiceType();
            }
        }
        
        protected PublishedContentUtility? pcUtil
        {
            get
            {
                if (_pcUtil == null)
                {
                    _pcUtil = _serviceUtil?.GetPublishedContentUtility();
                }
                return _pcUtil;
            }
        }
        protected MultiUrlUtility? urlUtil
        {
            get
            {
                if (_urlUtil == null)
                {
                    _urlUtil = _serviceUtil?.GetMultiUrlUtility();
                }
                return _urlUtil;
            }
        }
        protected MediaUtility? mediaUtil
        {
            get
            {
                if (_mediaUtil == null)
                {
                    _mediaUtil = _serviceUtil?.GetMediaUtility();
                }
                return _mediaUtil;
            }
        }
        protected ContentUtility? contentUtil
        {
            get
            {
                if (_contentUtil == null)
                {
                    _contentUtil = _serviceUtil?.GetContentUtility();
                }
                return _contentUtil;
            }
        }
        protected QueryUtility? queryUtil
        {
            get
            {
                if (_queryUtil == null)
                {
                    _queryUtil = _serviceUtil?.GetQueryUtility();
                }
                return _queryUtil;
            }
        }
    }
}

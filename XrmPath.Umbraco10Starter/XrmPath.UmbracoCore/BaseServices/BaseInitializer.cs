using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Security;
using XrmPath.UmbracoCore.Models;
using XrmPath.UmbracoCore.Utilities;

namespace XrmPath.UmbracoCore.BaseServices
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
        protected MemberUtility? _memberUtil;

        protected UmbracoHelper? umbracoHelper;
        protected IExamineManager? examineManager;
        protected IMediaService? mediaService;
        protected IContentService? contentService;
        protected IContentTypeService? contentTypeService;
        protected IMemberSignInManager? memberSignInManager;
        protected ILogger<object>? logger;
        protected AppSettingsModel? appSettings;

        public BaseInitializer(ServiceUtility? serviceUtil)
        {
            if (_serviceUtil == null && serviceUtil != null)
            {
                _serviceUtil = serviceUtil;
            }
            if (logger == null)
            {
                logger = _serviceUtil?.GetLogger();
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
            if (memberSignInManager == null)
            {
                memberSignInManager = _serviceUtil?.GetMemberSignInManager();
            }
            if (memberSignInManager == null)
            {
                memberSignInManager = _serviceUtil?.GetMemberSignInManager();
            }
            if (appSettings == null)
            {
                appSettings = _serviceUtil?.GetAppSettings();
            }
        }
        protected LoggingUtility? loggingUtil
        {
            get
            {
                if (_loggingUtil == null)
                {
                    _loggingUtil = _serviceUtil?.GetLoggingUtility();
                }
                return _loggingUtil;
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
        protected MemberUtility? memberUtil
        {
            get
            {
                if (_memberUtil == null)
                {
                    _memberUtil = _serviceUtil?.GetMemberUtility();
                }
                return _memberUtil;
            }
        }
    }
}

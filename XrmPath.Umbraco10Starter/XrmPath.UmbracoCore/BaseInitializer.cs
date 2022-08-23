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
        public BaseInitializer(ServiceUtility? serviceUtil) {
            if (_serviceUtil == null && serviceUtil != null)
            {
                _serviceUtil = serviceUtil;
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

    }
}

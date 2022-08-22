using Examine;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;

namespace XrmPath.Web.UmbracoUtils
{
    public abstract class BaseUtility
    {
        protected readonly UmbracoHelper? _umbracoHelper;
        protected readonly IMediaService? _mediaService;
        protected readonly IExamineManager? _examineManager;
        public BaseUtility(UmbracoHelper? umbracoHelper, IMediaService? mediaService, IExamineManager? examineManager)
        {
            if (umbracoHelper != null && _umbracoHelper == null) {
                _umbracoHelper = umbracoHelper;
            }
            if (mediaService != null && _mediaService == null) {
                _mediaService = mediaService;
            }
            if (examineManager != null && _examineManager == null)
            {
                _examineManager = examineManager;
            }
        }
    }
}

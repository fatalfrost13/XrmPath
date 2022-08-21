using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;

namespace XrmPath.Web.UmbracoUtils
{
    public abstract class BaseUtility
    {
        protected readonly UmbracoHelper? _umbracoHelper;
        protected readonly IMediaService? _mediaService;
        public BaseUtility(UmbracoHelper? umbracoHelper, IMediaService? mediaService)
        {
            if (umbracoHelper != null) {
                _umbracoHelper = umbracoHelper;
            }
            if (mediaService != null) {
                _mediaService = mediaService;
            }
        }
    }
}

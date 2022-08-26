using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Security;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.Utilities
{
    public interface IServiceUtility
    {
        public PublishedContentUtility? GetPublishedContentUtility();
        UmbracoHelper? GetUmbracoHelper();
        public IMediaService? GetMediaService();
        public IExamineManager? GetExamineManager();
        public IContentService? GetContentService();
        public IContentTypeService? GetContentServiceType();
        public IMemberManager? GetMemberManager();
        public IMemberSignInManager? GetMemberSignInManager();
        public ILogger<object>? GetLogger();
        public AppSettingsModel? GetAppSettings();
        public LoggingUtility? GetLoggingUtility();
        public MultiUrlUtility? GetMultiUrlUtility();
        public ContentUtility? GetContentUtility();
        public MediaUtility? GetMediaUtility();
        public SearchUtility? GetSearchUtility();
        public QueryUtility? GetQueryUtility();
        public MemberUtility? GetMemberUtility();
        public MembershipUtility? GetMembershipUtility();
    }
}

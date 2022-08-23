using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common;
using Examine;
using XrmPath.UmbracoCore.Models;
using Umbraco.Cms.Core.Services;
using XrmPath.UmbracoCore.Utilities;

namespace XrmPath.Web.Controllers
{
    [Route("Search/[action]")]
    public class SearchController : Controller
    {
        //private readonly IExamineManager _examineIndex;
        private readonly ServiceUtility? _serviceUtil;
        private readonly SearchUtility? _searchUtil;
        private readonly PublishedContentUtility? _pcUtil;
        //private readonly UmbracoHelper? _umbracoHelper;
        //private readonly IMediaService? _mediaService;

        public SearchController(ILogger<object> iLogger, UmbracoHelper umbracoHelper, IMediaService mediaService, IExamineManager examineIndex) {

            if (_serviceUtil == null) {
                _serviceUtil = new ServiceUtility(iLogger, umbracoHelper, mediaService, examineIndex);
                _pcUtil = _serviceUtil?.GetPublishedContentUtility();
                _searchUtil = _serviceUtil?.GetSearchUtility();
            }
        }

        public SearchResultItemPager? GetSearchResults(string searchterm, int pagesize, int currentpage)
        {
            var results = _searchUtil?.GetSearchResultPager(searchterm, pagesize, currentpage);
            return results;
        }

    }
}
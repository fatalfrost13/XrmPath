using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common;
using Examine;
using XrmPath.UmbracoCore.Models;
using Umbraco.Cms.Core.Services;
using XrmPath.UmbracoCore.Utilities;
using XrmPath.UmbracoCore;

namespace XrmPath.Web.Controllers
{
    [Route("Search/[action]")]
    public class SearchController : Controller
    {
        //private readonly IExamineManager _examineIndex;
        private readonly ServiceUtility? _serviceUtil;
        private readonly SearchUtility? _searchUtil;
        private readonly PublishedContentUtility? _pcUtil;
        private readonly LoggingUtility? _loggingUtil;
        //private readonly UmbracoHelper? _umbracoHelper;
        //private readonly IMediaService? _mediaService;

        public SearchController(ILogger<object> logger, UmbracoHelper umbracoHelper, IMediaService mediaService, IExamineManager examineIndex) {

            if (_serviceUtil == null) {
                _serviceUtil = new ServiceUtility(logger, umbracoHelper, mediaService, examineIndex);
                _pcUtil = _serviceUtil?.GetPublishedContentUtility();
                _searchUtil = _serviceUtil?.GetSearchUtility();
                _loggingUtil = _serviceUtil?.GetLoggingUtility();
            }
        }

        public SearchResultItemPager? GetSearchResults(string searchterm, int pagesize, int currentpage)
        {
            //_loggingUtil?.Information("DOES THIS WORK?!?!");
            var results = _searchUtil?.GetSearchResultPager(searchterm, pagesize, currentpage);
            return results;
        }

    }
}
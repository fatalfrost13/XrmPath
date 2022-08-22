using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using Examine;
using XrmPath.UmbracoUtils.Models;
using XrmPath.UmbracoUtils;
using Umbraco.Cms.Core.Services;

namespace XrmPath.Web.Controllers
{
    [Route("Search/[action]")]
    public class SearchController : Controller
    {
        private readonly IExamineManager _examineIndex;
        private readonly SearchUtility _searchUtil;
        private readonly PublishedContentUtility _pcUtil;
        private readonly UmbracoHelper? _umbracoHelper;
        private readonly IMediaService? _mediaService;

        public SearchController(UmbracoHelper umbracoHelper, IMediaService mediaService, IExamineManager examineIndex) {
            if (_pcUtil == null) {
                _pcUtil = new PublishedContentUtility(umbracoHelper, mediaService, examineIndex);
                _searchUtil = new SearchUtility(_pcUtil);
            }
        }

        public SearchResultItemPager GetSearchResults(string searchterm, int pagesize, int currentpage)
        {
            var results = _searchUtil.GetSearchResultPager(searchterm, pagesize, currentpage);
            return results;
        }

    }
}
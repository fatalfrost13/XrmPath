using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;

namespace XrmPath.Web.Controllers
{
    [Route("TestController/[action]")]
    public class TestController : Controller
    {
        private readonly UmbracoHelper _umbracoHelper;

        public TestController(UmbracoHelper umbracoHelper) => _umbracoHelper = umbracoHelper;

        public IActionResult GetHomeNodeName()
        {
            IPublishedContent? rootNode = _umbracoHelper.ContentAtRoot().FirstOrDefault();

            if (rootNode is null)
            {
                return NotFound();
            }

            return Ok(rootNode.Name);
        }
    }
}